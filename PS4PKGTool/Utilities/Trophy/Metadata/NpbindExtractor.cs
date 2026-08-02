#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PS4PKGTool.Utilities.TrophyMetadata
{
    public sealed class NpbindExtractionResult
    {
        public string? NpCommunicationId { get; init; }
        public string ErrorMessage { get; init; } = string.Empty;
        public bool Succeeded => NpCommunicationId != null;
    }

    /// <summary>Extracts only Sc0/npbind.dat and discards the temporary binary after reading its NPWR token.</summary>
    public sealed class NpbindExtractor
    {
        private const string DefaultPasscode = "00000000000000000000000000000000";
        private readonly NpCommunicationIdResolver _resolver = new();

        public async Task<NpbindExtractionResult> ExtractAsync(
            string orbisPubCmdPath,
            string pkgPath,
            string temporaryRoot,
            CancellationToken cancellationToken = default)
        {
            if (!File.Exists(orbisPubCmdPath))
                return Failure("orbis-pub-cmd.exe was not found.");
            if (!File.Exists(pkgPath))
                return Failure("The selected PKG was not found.");

            string root = Path.GetFullPath(temporaryRoot);
            Directory.CreateDirectory(root);
            string workDirectory = Path.Combine(root, "npbind_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(workDirectory);

            string originalPkgPath = Path.GetFullPath(pkgPath);
            string packageDirectory = Path.GetDirectoryName(originalPkgPath)
                ?? throw new InvalidOperationException("The selected PKG has no parent directory.");
            string safePkgPath = Path.Combine(packageDirectory, "ps4pkgtool_npbind_" + Guid.NewGuid().ToString("N") + ".pkg");
            bool renamed = false;
            string? restorationError = null;
            NpbindExtractionResult result;

            try
            {
                // Rename in place so even very large PKGs get an ASCII-only path without copying.
                // The unique target is in the exact same parent directory and is restored below.
                File.Move(originalPkgPath, safePkgPath);
                renamed = true;
                result = await RunOrbisAsync(
                    Path.GetFullPath(orbisPubCmdPath), safePkgPath, workDirectory, root, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException or System.ComponentModel.Win32Exception)
            {
                result = Failure(ex.Message);
            }
            finally
            {
                if (renamed && File.Exists(safePkgPath))
                {
                    try
                    {
                        if (File.Exists(originalPkgPath))
                            restorationError = "The original PKG path unexpectedly exists; the temporary PKG was preserved at " + safePkgPath;
                        else
                            File.Move(safePkgPath, originalPkgPath);
                    }
                    catch (Exception ex)
                    {
                        restorationError = "Failed to restore the original PKG filename. Recover it from " + safePkgPath + ". " + ex.Message;
                    }
                }

                // workDirectory is a GUID child created above; never delete the caller-provided root.
                string relative = Path.GetRelativePath(root, workDirectory);
                if (!relative.StartsWith("..", StringComparison.Ordinal) && Directory.Exists(workDirectory))
                {
                    try { Directory.Delete(workDirectory, recursive: true); } catch { }
                }
            }

            return restorationError == null ? result : Failure(restorationError);
        }

        private async Task<NpbindExtractionResult> RunOrbisAsync(
            string orbisPubCmdPath,
            string safePkgPath,
            string workDirectory,
            string fallbackWorkingDirectory,
            CancellationToken cancellationToken)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = orbisPubCmdPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(orbisPubCmdPath) ?? fallbackWorkingDirectory
                }
            };
            process.StartInfo.ArgumentList.Add("img_extract");
            process.StartInfo.ArgumentList.Add("--passcode");
            process.StartInfo.ArgumentList.Add(DefaultPasscode);
            process.StartInfo.ArgumentList.Add(safePkgPath + ":Sc0/npbind.dat");
            process.StartInfo.ArgumentList.Add(workDirectory);

            process.Start();
            Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
            Task<string> stderrTask = process.StandardError.ReadToEndAsync();

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(30));
            try
            {
                await process.WaitForExitAsync(timeout.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                try
                {
                    if (!process.HasExited) process.Kill(entireProcessTree: true);
                    process.WaitForExit();
                }
                catch { }
                await Task.WhenAll(stdoutTask, stderrTask).ConfigureAwait(false);
                return Failure(cancellationToken.IsCancellationRequested
                    ? "NP Communication ID extraction was cancelled."
                    : "NP Communication ID extraction timed out.");
            }

            string stdout = await stdoutTask.ConfigureAwait(false);
            string stderr = await stderrTask.ConfigureAwait(false);
            string? npbindPath = Directory.EnumerateFiles(workDirectory, "npbind.dat", SearchOption.AllDirectories)
                .FirstOrDefault();
            if (npbindPath == null)
                return Failure(DescribeFailure(process.ExitCode, stdout, stderr));

            string? id = _resolver.ResolveFromFile(npbindPath);
            return id == null
                ? Failure("Sc0/npbind.dat was extracted, but no valid NPWRxxxxx_00 value was found.")
                : new NpbindExtractionResult { NpCommunicationId = id };
        }

        private static string DescribeFailure(int exitCode, string stdout, string stderr)
        {
            string detail = string.Join(" ", new[] { stderr, stdout }
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim().Replace('\r', ' ').Replace('\n', ' ')));
            if (detail.Length > 500) detail = detail[..500];
            return string.IsNullOrEmpty(detail)
                ? $"orbis-pub-cmd exited with code {exitCode} without producing npbind.dat."
                : $"orbis-pub-cmd exited with code {exitCode}: {detail}";
        }

        private static NpbindExtractionResult Failure(string message) =>
            new() { ErrorMessage = message };
    }
}
