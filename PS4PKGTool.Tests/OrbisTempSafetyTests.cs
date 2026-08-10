using Microsoft.VisualStudio.TestTools.UnitTesting;
using PS4PKGTool.Utilities.PS4PKGToolHelper;

namespace PS4PKGTool.Tests;

[TestClass]
public class OrbisTempSafetyTests
{
    private string _dir = null!;

    [TestInitialize]
    public void Init()
    {
        _dir = Path.Combine(Path.GetTempPath(), "p4t_test_" + Guid.NewGuid().ToString("N").Substring(0, 8));
        Directory.CreateDirectory(_dir);
    }

    [TestCleanup]
    public void Cleanup()
    {
        try
        {
            if (Directory.Exists(_dir))
                Directory.Delete(_dir, true);
        }
        catch { /* best effort */ }
    }

    [TestMethod]
    public void BeginRename_WritesSidecar_AndMovesFile()
    {
        string orig = Path.Combine(_dir, "Game Title.pkg");
        string temp = Path.Combine(_dir, OrbisTempSafety.TempPkgPrefix + "abc123.pkg");
        File.WriteAllBytes(orig, new byte[] { 10, 20, 30 });

        OrbisTempSafety.BeginOrbisPkgRename(orig, temp);

        Assert.IsFalse(File.Exists(orig), "original should be moved away");
        Assert.IsTrue(File.Exists(temp), "temp PKG should exist");
        string side = OrbisTempSafety.SidecarPath(temp);
        Assert.IsTrue(File.Exists(side), "restore sidecar should exist");
        Assert.AreEqual(orig, File.ReadAllText(side).Trim());
    }

    [TestMethod]
    public void EndRestore_MovesBack_AndRemovesSidecar()
    {
        string orig = Path.Combine(_dir, "Restore Me.pkg");
        string temp = Path.Combine(_dir, OrbisTempSafety.TempPkgPrefix + "def456.pkg");
        File.WriteAllBytes(orig, new byte[] { 1, 2, 3, 4 });
        OrbisTempSafety.BeginOrbisPkgRename(orig, temp);

        OrbisTempSafety.EndOrbisPkgRestore(temp, orig);

        Assert.IsTrue(File.Exists(orig));
        Assert.IsFalse(File.Exists(temp));
        Assert.IsFalse(File.Exists(OrbisTempSafety.SidecarPath(temp)));
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4 }, File.ReadAllBytes(orig));
    }

    [TestMethod]
    public void RecoverOrphans_RestoresUsingSidecar()
    {
        string orig = Path.Combine(_dir, "Crashed Game.pkg");
        string temp = Path.Combine(_dir, OrbisTempSafety.TempPkgPrefix + Guid.NewGuid().ToString("N") + ".pkg");
        File.WriteAllBytes(orig, new byte[] { 9, 9, 9 });
        OrbisTempSafety.BeginOrbisPkgRename(orig, temp);

        // Simulate crash: leave temp + sidecar, do not call EndRestore
        Assert.IsFalse(File.Exists(orig));
        Assert.IsTrue(File.Exists(temp));

        int n = OrbisTempSafety.RecoverOrphanedPkgs(new[] { _dir });
        Assert.IsTrue(n >= 1, "should restore at least one orphan");
        Assert.IsTrue(File.Exists(orig), "original path restored");
        Assert.IsFalse(File.Exists(temp), "temp name gone");
        Assert.IsFalse(File.Exists(OrbisTempSafety.SidecarPath(temp)));
    }

    [TestMethod]
    public void RecoverOrphans_WhenOriginalExists_LeavesTemp()
    {
        string orig = Path.Combine(_dir, "Already There.pkg");
        string temp = Path.Combine(_dir, OrbisTempSafety.TempPkgPrefix + "conflict.pkg");
        File.WriteAllBytes(orig, new byte[] { 1 });
        File.WriteAllBytes(temp, new byte[] { 2, 2 });
        File.WriteAllText(OrbisTempSafety.SidecarPath(temp), orig);

        int n = OrbisTempSafety.RecoverOrphanedPkgs(new[] { _dir });
        // Should not overwrite existing original
        Assert.IsTrue(File.Exists(orig));
        Assert.IsTrue(File.Exists(temp), "temp should remain when original exists");
        CollectionAssert.AreEqual(new byte[] { 1 }, File.ReadAllBytes(orig));
        Assert.IsTrue(n == 0 || File.Exists(temp)); // recovery count 0 for this case
    }

    [TestMethod]
    public void RecoverOrphans_FindsTempInsideP4tSubfolder()
    {
        string sub = Path.Combine(_dir, "p4t_v_abc123");
        Directory.CreateDirectory(sub);
        string orig = Path.Combine(_dir, "Nested.pkg");
        string temp = Path.Combine(sub, OrbisTempSafety.TempPkgPrefix + "nested.pkg");
        File.WriteAllBytes(orig, new byte[] { 7, 7 });
        OrbisTempSafety.BeginOrbisPkgRename(orig, temp);

        int n = OrbisTempSafety.RecoverOrphanedPkgs(new[] { _dir });
        Assert.IsTrue(n >= 1);
        Assert.IsTrue(File.Exists(orig));
        Assert.IsFalse(File.Exists(temp));
    }

    [TestMethod]
    public void CleanupStaleTempDirectories_RemovesOldP4tFolders()
    {
        // Use system temp so CleanupStaleTempDirectories scans it
        string stale = Path.Combine(Path.GetTempPath(), "p4t_z_" + Guid.NewGuid().ToString("N").Substring(0, 6));
        Directory.CreateDirectory(stale);
        File.WriteAllText(Path.Combine(stale, "junk.txt"), "x");
        Directory.SetLastWriteTimeUtc(stale, DateTime.UtcNow.AddHours(-12));
        Directory.SetCreationTimeUtc(stale, DateTime.UtcNow.AddHours(-12));

        string fresh = Path.Combine(Path.GetTempPath(), "p4t_z_" + Guid.NewGuid().ToString("N").Substring(0, 6));
        Directory.CreateDirectory(fresh);
        File.WriteAllText(Path.Combine(fresh, "new.txt"), "y");

        try
        {
            int cleaned = OrbisTempSafety.CleanupStaleTempDirectories(TimeSpan.FromHours(6));
            Assert.IsTrue(cleaned >= 1, "expected at least one stale folder removed");
            Assert.IsFalse(Directory.Exists(stale), "stale p4t folder should be deleted");
            Assert.IsTrue(Directory.Exists(fresh), "fresh p4t folder should remain");
        }
        finally
        {
            try { if (Directory.Exists(fresh)) Directory.Delete(fresh, true); } catch { }
            try { if (Directory.Exists(stale)) Directory.Delete(stale, true); } catch { }
        }
    }

    [TestMethod]
    public void CleanupStaleTempDirectories_SkipsFolderWithActiveSidecar()
    {
        string active = Path.Combine(Path.GetTempPath(), "p4t_a_" + Guid.NewGuid().ToString("N").Substring(0, 6));
        Directory.CreateDirectory(active);
        string tempPkg = Path.Combine(active, OrbisTempSafety.TempPkgPrefix + "live.pkg");
        File.WriteAllBytes(tempPkg, new byte[] { 3 });
        File.WriteAllText(OrbisTempSafety.SidecarPath(tempPkg), Path.Combine(_dir, "live-orig.pkg"));
        Directory.SetLastWriteTimeUtc(active, DateTime.UtcNow.AddHours(-12));
        Directory.SetCreationTimeUtc(active, DateTime.UtcNow.AddHours(-12));

        try
        {
            OrbisTempSafety.CleanupStaleTempDirectories(TimeSpan.FromHours(6));
            Assert.IsTrue(Directory.Exists(active), "folder with active sidecar must not be deleted");
            Assert.IsTrue(File.Exists(tempPkg));
        }
        finally
        {
            try { Directory.Delete(active, true); } catch { }
        }
    }

    [TestMethod]
    public void HasEnoughDiskSpace_TinyPkg_Succeeds()
    {
        string pkg = Path.Combine(_dir, "tiny.pkg");
        File.WriteAllBytes(pkg, new byte[4096]);
        bool ok = OrbisTempSafety.HasEnoughDiskSpaceForExtract(pkg, _dir, out string? msg);
        Assert.IsTrue(ok, msg ?? "expected enough space for 4KB pkg");
        Assert.IsTrue(string.IsNullOrEmpty(msg));
    }

    [TestMethod]
    public void HasEnoughDiskSpace_ZeroLengthPkg_Fails()
    {
        string pkg = Path.Combine(_dir, "empty.pkg");
        File.WriteAllBytes(pkg, Array.Empty<byte>());
        bool ok = OrbisTempSafety.HasEnoughDiskSpaceForExtract(pkg, _dir, out string? msg);
        Assert.IsFalse(ok);
        Assert.IsFalse(string.IsNullOrEmpty(msg));
    }

    [TestMethod]
    public void SafeMoveFile_SameVolume_Works()
    {
        string a = Path.Combine(_dir, "a.bin");
        string b = Path.Combine(_dir, "b.bin");
        File.WriteAllBytes(a, new byte[] { 5, 5, 5 });
        OrbisTempSafety.SafeMoveFile(a, b);
        Assert.IsFalse(File.Exists(a));
        Assert.IsTrue(File.Exists(b));
        CollectionAssert.AreEqual(new byte[] { 5, 5, 5 }, File.ReadAllBytes(b));
    }

    [TestMethod]
    public void EndRestore_Idempotent_WhenAlreadyRestored()
    {
        string orig = Path.Combine(_dir, "Idem.pkg");
        string temp = Path.Combine(_dir, OrbisTempSafety.TempPkgPrefix + "idem.pkg");
        File.WriteAllBytes(orig, new byte[] { 8 });
        OrbisTempSafety.BeginOrbisPkgRename(orig, temp);
        OrbisTempSafety.EndOrbisPkgRestore(temp, orig);
        // Second call must not throw
        OrbisTempSafety.EndOrbisPkgRestore(temp, orig);
        Assert.IsTrue(File.Exists(orig));
    }
}
