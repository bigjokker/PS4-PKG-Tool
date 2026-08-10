using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PS4PKGTool;
using PS4PKGTool.Util;

namespace PS4PKGTool.Utilities.PS4PKGToolHelper
{
    /// <summary>
    /// Reliability helpers for orbis-pub-cmd temp renames: restore sidecars, orphan recovery,
    /// stale temp cleanup, and disk-space checks before large extracts.
    /// </summary>
    public static class OrbisTempSafety
    {
        public const string TempPkgPrefix = "ps4pkgtool_orbis_";
        public const string RestoreSidecarSuffix = ".restore";

        /// <summary>Move a file, falling back to copy+delete when volumes differ.</summary>
        public static void SafeMoveFile(string src, string dst)
        {
            try
            {
                if (File.Exists(dst)) File.Delete(dst);
                File.Move(src, dst);
            }
            catch (IOException)
            {
                File.Copy(src, dst, overwrite: true);
                try { File.Delete(src); } catch { /* leave source if delete fails after copy */ }
            }
        }

        public static string SidecarPath(string tempPkgPath) => tempPkgPath + RestoreSidecarSuffix;

        /// <summary>Rename PKG for orbis and record original path for crash recovery.</summary>
        public static void BeginOrbisPkgRename(string originalPath, string tempPkgPath)
        {
            SafeMoveFile(originalPath, tempPkgPath);
            try
            {
                File.WriteAllText(SidecarPath(tempPkgPath), originalPath ?? "");
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Could not write PKG restore sidecar: " + ex.Message);
            }
        }

        /// <summary>Move PKG back to original path and remove sidecar.</summary>
        public static void EndOrbisPkgRestore(string tempPkgPath, string originalPath)
        {
            try
            {
                if (File.Exists(tempPkgPath))
                {
                    if (File.Exists(originalPath))
                        Logger.LogError("Failed to restore PKG filename because the original path already exists. Recover the PKG from: " + tempPkgPath);
                    else
                        SafeMoveFile(tempPkgPath, originalPath);
                }
            }
            finally
            {
                TryDeleteSidecar(tempPkgPath);
            }
        }

        public static void TryDeleteSidecar(string tempPkgPath)
        {
            try
            {
                string side = SidecarPath(tempPkgPath);
                if (File.Exists(side)) File.Delete(side);
            }
            catch { }
        }

        /// <summary>
        /// Find leftover ps4pkgtool_orbis_*.pkg files and restore them using sidecars,
        /// or rename via Title ID when no sidecar exists.
        /// </summary>
        public static int RecoverOrphanedPkgs(IEnumerable<string> searchRoots)
        {
            int recovered = 0;
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string orphan in EnumerateTempPkgs(searchRoots))
            {
                if (!seen.Add(orphan)) continue;
                try
                {
                    // If a sidecar exists, only attempt path restore — never fall through to
                    // Title-ID rename (that would "recover" a conflict temp into a new name).
                    if (File.Exists(SidecarPath(orphan)))
                    {
                        if (TryRestoreFromSidecar(orphan))
                            recovered++;
                        continue;
                    }
                    if (TryRecoverWithoutSidecar(orphan))
                        recovered++;
                }
                catch (Exception ex)
                {
                    Logger.LogWarning("Orphan PKG recovery failed for " + orphan + ": " + ex.Message);
                }
            }
            return recovered;
        }

        /// <summary>Delete stale p4t_* temp directories older than <paramref name="maxAge"/>.</summary>
        public static int CleanupStaleTempDirectories(TimeSpan maxAge)
        {
            int removed = 0;
            DateTime cutoff = DateTime.UtcNow - maxAge;
            var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try { roots.Add(Path.GetFullPath(Path.GetTempPath())); } catch { }
            try { roots.Add(Path.GetFullPath(Helper.GetAsciiTempRoot())); } catch { }
            try
            {
                string common = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "PS4PKGTool", "tmp");
                roots.Add(Path.GetFullPath(common));
            }
            catch { }
            try
            {
                string sysRoot = Path.GetPathRoot(Environment.SystemDirectory);
                if (!string.IsNullOrEmpty(sysRoot))
                    roots.Add(Path.GetFullPath(Path.Combine(sysRoot, "p4t_tmp")));
            }
            catch { }

            foreach (string root in roots)
            {
                if (!Directory.Exists(root)) continue;
                IEnumerable<string> dirs;
                try
                {
                    dirs = Directory.EnumerateDirectories(root, "p4t_*", SearchOption.TopDirectoryOnly);
                }
                catch { continue; }

                foreach (string dir in dirs)
                {
                    try
                    {
                        var info = new DirectoryInfo(dir);
                        // Keep folders touched within maxAge (active or recent jobs)
                        if (info.LastWriteTimeUtc > cutoff)
                            continue;
                        // Only remove if no live rename sidecars (active job)
                        bool hasActiveSidecar = Directory.EnumerateFiles(dir, TempPkgPrefix + "*" + RestoreSidecarSuffix, SearchOption.TopDirectoryOnly).Any();
                        if (hasActiveSidecar) continue;

                        Directory.Delete(dir, recursive: true);
                        removed++;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning("Temp cleanup skipped " + dir + ": " + ex.Message);
                    }
                }
            }
            return removed;
        }

        /// <summary>
        /// Ensure enough free space for a full extract. Uses a conservative multiple of PKG size
        /// on the temp volume and the destination volume.
        /// </summary>
        public static bool HasEnoughDiskSpaceForExtract(string pkgPath, string destinationDirectory, out string message)
        {
            message = null;
            try
            {
                long pkgSize = new FileInfo(pkgPath).Length;
                if (pkgSize <= 0)
                {
                    message = "PKG file size is zero or unreadable.";
                    return false;
                }

                // Extracted content is often larger than the PKG; temp + final can both hold a full tree.
                long needPerVolume = checked(pkgSize * 2 + 64L * 1024 * 1024); // 2× + 64 MB headroom

                string tempRoot = Helper.GetAsciiTempRoot(pkgPath);
                string tempVol = Path.GetPathRoot(Path.GetFullPath(tempRoot));
                string destVol = Path.GetPathRoot(Path.GetFullPath(destinationDirectory));

                long freeTemp = GetAvailableBytes(tempVol);
                long freeDest = GetAvailableBytes(destVol);

                bool sameVolume = !string.IsNullOrEmpty(tempVol) && string.Equals(tempVol, destVol, StringComparison.OrdinalIgnoreCase);
                if (sameVolume)
                {
                    long need = checked(pkgSize * 3 + 64L * 1024 * 1024);
                    if (freeTemp < need)
                    {
                        message = $"Not enough free space on {tempVol}. Need about {Helper.RoundBytes(need)}, have {Helper.RoundBytes(freeTemp)}.";
                        return false;
                    }
                }
                else
                {
                    if (freeTemp < needPerVolume)
                    {
                        message = $"Not enough free space on temp drive {tempVol}. Need about {Helper.RoundBytes(needPerVolume)}, have {Helper.RoundBytes(freeTemp)}.";
                        return false;
                    }
                    if (freeDest < needPerVolume)
                    {
                        message = $"Not enough free space on destination drive {destVol}. Need about {Helper.RoundBytes(needPerVolume)}, have {Helper.RoundBytes(freeDest)}.";
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                // Don't block extract if free-space probe fails (e.g. odd network path)
                Logger.LogWarning("Disk space check skipped: " + ex.Message);
                message = null;
                return true;
            }
        }

        private static long GetAvailableBytes(string volumeRoot)
        {
            if (string.IsNullOrEmpty(volumeRoot)) return long.MaxValue;
            var di = new DriveInfo(volumeRoot);
            return di.IsReady ? di.AvailableFreeSpace : long.MaxValue;
        }

        private static IEnumerable<string> EnumerateTempPkgs(IEnumerable<string> searchRoots)
        {
            var list = new List<string>();
            foreach (string root in searchRoots.Where(r => !string.IsNullOrWhiteSpace(r)))
            {
                string full;
                try { full = Path.GetFullPath(root); } catch { continue; }
                if (!Directory.Exists(full)) continue;

                CollectInDirectory(full, list);
                try
                {
                    foreach (string sub in Directory.EnumerateDirectories(full, "p4t_*", SearchOption.TopDirectoryOnly))
                        CollectInDirectory(sub, list);
                }
                catch { }
            }
            return list;
        }

        private static void CollectInDirectory(string dir, List<string> list)
        {
            try
            {
                list.AddRange(Directory.EnumerateFiles(dir, TempPkgPrefix + "*.pkg", SearchOption.TopDirectoryOnly));
            }
            catch { }
        }

        private static bool TryRestoreFromSidecar(string tempPkgPath)
        {
            string side = SidecarPath(tempPkgPath);
            if (!File.Exists(side)) return false;

            string original;
            try { original = File.ReadAllText(side).Trim(); }
            catch { return false; }
            if (string.IsNullOrEmpty(original)) return false;

            string destDir = Path.GetDirectoryName(original);
            if (string.IsNullOrEmpty(destDir) || !Directory.Exists(destDir))
            {
                Logger.LogWarning("Orphan PKG sidecar points to missing directory: " + original);
                return false;
            }

            if (File.Exists(original))
            {
                Logger.LogWarning("Orphan PKG original already exists, leaving temp file: " + tempPkgPath);
                return false;
            }

            SafeMoveFile(tempPkgPath, original);
            TryDeleteSidecar(tempPkgPath);
            Logger.LogInformation("Restored orphaned PKG to original path: " + original);
            return true;
        }

        private static bool TryRecoverWithoutSidecar(string tempPkgPath)
        {
            // No sidecar (older build / crash before write). Rename in-place using package metadata.
            try
            {
                var pkg = PS4_Tools.PKG.SceneRelated.Read_PKG(tempPkgPath);
                string titleId = pkg.Param?.TITLEID;
                if (string.IsNullOrWhiteSpace(titleId))
                    titleId = "UNKNOWN";
                string title = (pkg.PS4_Title ?? "recovered").SanitizeFileName();
                if (title.Length > 80) title = title.Substring(0, 80);

                string dir = Path.GetDirectoryName(tempPkgPath) ?? ".";
                string dest = Path.Combine(dir, $"{titleId} - {title}.pkg");
                int n = 1;
                while (File.Exists(dest))
                {
                    dest = Path.Combine(dir, $"{titleId} - {title} ({n}).pkg");
                    n++;
                }

                SafeMoveFile(tempPkgPath, dest);
                TryDeleteSidecar(tempPkgPath);
                Logger.LogInformation("Recovered orphaned PKG without sidecar as: " + dest);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Could not rename orphan PKG " + tempPkgPath + ": " + ex.Message);
                return false;
            }
        }
    }
}
