# PS4PKGTool — Full Bug Analysis Report
## Generated 2026-07-26

---

## DATA LOSS (2)

| # | File | Line(s) | Issue |
|---|---|---|---|
| DL-1 | Main.cs | 4574-4596 | PKG renamed to `temp_ps4pkgsafe.pkg` for extraction. If app crashes mid-extraction, the original `.pkg` is permanently lost — file sits renamed but invisible to the `.PKG` scanner. Affects `ExtractFullPKG`, `ExtractSelectedPKGData`, and `ExtractFilesSync`. |
| DL-2 | Main.cs | 4577, 4596 | Both rename operations wrapped in `try { } catch { }` — if rename-back fails, the PKG is silently lost with no error to the user. |

---

## CRITICAL / CRASH (10)

| # | File | Line(s) | Issue |
|---|---|---|---|
| C-1 | Helper.cs | 215-216 | `Update.Part` getter calls `return Part;` (the property itself) instead of `return Part_;` (the backing field). **Infinite recursion → StackOverflowException**. |
| C-2 | SettingsManager.cs | 20-21 | `File.Create(filePath)` returns undisposed `FileStream`, then `new StreamWriter(filePath)` tries to open same file → **IOException** from file lock. Settings can't be saved. |
| C-3 | Helper.cs | 866-875 | `GetPkgHeaderBuffer` uses a `private static byte[]` buffer shared across all threads. Concurrent calls overwrite each other's data. **Data corruption**. |
| C-4 | Main.cs | 300 | `async void Form1_Load` has no try-catch. Any unhandled exception in `Task.Run` crashes the **entire process**. |
| C-5 | Main.cs | 4365-4369 | `WaitForExit(7000)` return value discarded. If process hangs, `ReadToEnd()` deadlocks and `ExitCode` throws `InvalidOperationException`. |
| C-6 | Main.cs | 1857 | `PKG.VerifiedPs4PkgList` (shared `List<string>`) accessed from concurrent BackgroundWorkers without locks. Drag-drop during initial scan = list corruption. |
| C-7 | Main.cs | 380-385 | `Read_PKG(PKG.SelectedPKGFilename)` called without null/empty check. If `SelectFirstRowPkg` fails silently, NRE. |
| C-8 | Main.cs | 6401 | `listView1.SelectedItems[0].Tag` accessed without checking `SelectedItems.Count > 0`. Crash if list empty. |
| C-9 | Main.cs | 4618 | `Directory.Move` between different volumes (C: → D:) throws — cross-volume extraction fails. |
| C-10 | Main.cs | 322-329 | `while (BGM.isBGMPlaying) { ... }` infinite loop on UI thread if stop doesn't clear flag. |

---

## HIGH (16)

| # | File | Line(s) | Issue |
|---|---|---|---|
| H-1 | Main.cs | 1925, 2211 | **Copy-paste bug** — `neoEnable` ternary uses `psvr` instead of `neo`: `(psvr != "null") ? "NA" : ""`. PS4 Pro Enhanced column shows wrong value. |
| H-2 | Main.cs | 4586 | `ExtractFullPKG` uses `tbPasscode.Text` but other paths use `PKG.Passcode` — inconsistent passcode source. |
| H-3 | Main.cs | 6141 | Size column in DGV sorts **alphabetically** ("100 MB" before "4.50 GB") instead of numerically. |
| H-4 | Main.cs | 2095 | Direct-scan `dttemp` has 15 columns (no "Latest Update"). DGV never shows Latest Update after direct scan because `UpdateDataGridViewColumnVisibility` assumes 16 columns and `catch{}` swallows the `IndexOutOfRangeException`. |
| H-5 | Main.cs | 4822 | `ShowInformation` called inside extraction loop — extracting 100 files shows 100 MessageBox dialogs, blocking UI. |
| H-6 | Main.cs | 4357 | Command-line argument injection — `PKG.Passcode` and filename interpolated directly into orbis-pub-cmd args without escaping. |
| H-7 | Main.cs | 2959 | `DeletePkg` — `row.Cells[0].Value.ToString()` throws NRE if cell is null. No null guard. |
| H-8 | Main.cs | 2756-2762, 1977 | `SaveManifestAfterScan` called from concurrent paths (initial load completion + drag-drop completion) with no synchronization — manifest file corruption. |
| H-9 | SettingsManager.cs | 83 | Wrong substring for saved directory — strips `"saved_last_directory="` (20 chars) instead of `"saved_fbd_last_directory="` (26 chars). Result: `"fbd_last_directory=C:\..."` instead of `"C:\..."`. |
| H-10 | Helper.cs | 994 | `ExtractBgm` has `return;` inside nested `foreach` — exits entire method after first encrypted AT9 entry, skipping all other PKGs. |
| H-11 | Helper.cs | 952 | `break;` on existing AT9 file exits outer PKG loop — skips extraction for all subsequent PKGs. |
| H-12 | Helper.cs | 85-86 | `CheckPKGBackported` calls `File.ReadAllText(BackportInfoFile)` without `File.Exists` check — `FileNotFoundException` crash. |
| H-13 | Helper.cs | 489-491 | `HandleParentDirectoryNavigation` — checks `currentNode.Parent` before null-checking `currentNode`. NRE. |
| H-14 | Helper.cs | 181 | `ImageToBytes` uses `GetBuffer()` which returns internal buffer with trailing garbage, not `ToArray()`. Corrupted image bytes. |
| H-15 | Helper.cs | 468-469 | `TreeView.currentNode` and `rootNodes` are `static` fields shared across all TreeView instances. If multiple instances exist, they corrupt each other's state. |
| H-16 | Helper.cs | 1836-1846 | `PKGSENDER.UninstallGame` — no `WaitForExit` before reading `StandardOutput`. UI hangs if curl process stalls. |

---

## MEDIUM (15)

| # | File | Line(s) | Issue |
|---|---|---|---|
| M-1 | Main.cs | 121 | `PKGGridView.DataError += (_, args) => { args.Cancel = true; }` — all data binding errors permanently suppressed. Makes debugging nearly impossible. |
| M-2 | Main.cs | 2269 | `PKGGridView.DataSource = dttemp` reassigned inside every PKG loop iteration — grid rebuilds for every file. For 1000+ PKGs, massive UI lag. |
| M-3 | Main.cs | 4422 | `PKGTreeView.Invoke` called per tree node — for PKGs with thousands of files, thousands of cross-thread marshals. Should batch on UI thread. |
| M-4 | Main.cs | 2390 | `_regionLookup` lazy init not thread-safe. Two threads can race-initialize the dictionary. |
| M-5 | SettingsManager.cs | 72-265 | `LoadSettings` modifies shared static `appSettings_` directly — no list clear, partial failure leaves corrupted state, not thread-safe. |
| M-6 | SettingsManager.cs | 14 | Public static mutable field `appSettings_` — any code can replace without synchronization. |
| M-7 | ManifestHelper.cs | 220 | `new ImageConverter()` created per row during save, never disposed. GDI handle leak over time. |
| M-8 | Helper.cs | 1110-1111 | `HttpClient` instantiated per `DownloadFileFromUrlAsync` call — socket exhaustion under repeated use. |
| M-9 | Helper.cs | 162-174 | `BytesToImage` — `MemoryStream` not stored, eligible for GC while Image still alive per GDI+ requirements. Image corruption possible. |
| M-10 | Helper.cs | 1207 | `Ping myPing = new Ping()` not wrapped in `using` — `Ping` implements `IDisposable`. |
| M-11 | Helper.cs | 1123-1132 | URL interpolated into `cmd /c start` — characters like `&`, `|`, `;` cause arbitrary command execution. |
| M-12 | Helper.cs | 1563 | `StackTrace.GetFrame(1)` to detect caller — fragile, breaks if called through wrapper. |
| M-13 | Helper.cs | Throughout | Pervasive mutable static state with no synchronization (`FirstLaunch`, `LoadFromManifest`, `LaunchEmpty`, `CancelExtract`, `PKGSenderisDone_`, etc.). Background threads race with UI thread. |

---

## LOW (9)

| # | File | Line(s) | Issue |
|---|---|---|---|
| L-1 | Main.cs | 57 | `internal static string filenameDLC` — static field overwritten per PKG, only last value survives. |
| L-2 | Main.cs | 2103 | `hexOutput.Substring(0, 3)` — throws if hex string < 3 chars (zero/minimal SYSTEM_VER). |
| L-3 | Main.cs | 146 | Double `new Bitmap()` in icon loading — second bitmap leaked if `imageList1.Images.Add` throws. |
| L-4 | AppSettings.cs | 38 | `pkgTitleColumn` declared but never serialized by SaveSettings/LoadSettings — dead property. |
| L-5 | Helper.cs | 657-665 | `IsPkgGamePatchAppUnknown` returns `true` for everything except Addon — misleading name. |
| L-6 | SettingsManager.cs | 100 | `StartsWith("auto_sort_row")` missing `=` — overly broad match. |
| L-7 | Helper.cs | 52 | `RoundBytes(long)` returns negative for negative input instead of clamping to 0. |
| L-8 | Helper.cs | 1611, 1805 | Comment says "2 seconds timeout" but code passes 7000ms (7 seconds). |
| L-9 | Main.cs | 2114 | `byte[] bufferA = new byte[16]; bufferA = PKG.GetPkgHeaderBuffer(item);` — first allocation immediately discarded. |

---

## Totals

| Severity | Count |
|---|---|
| Data Loss | 2 |
| Critical/Crash | 10 |
| High | 16 |
| Medium | 15 |
| Low | 9 |
| **Total** | **52** |
