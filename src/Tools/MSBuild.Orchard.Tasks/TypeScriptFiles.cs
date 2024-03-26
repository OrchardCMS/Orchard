// Originally taken from https://msbuildtypescript.codeplex.com (no longer exists) under Apache 2.0.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuild.Orchard.Tasks {
    public class CompileTypeScriptFiles : Task {
        private static readonly Regex errorLineMatcher = new Regex(@"^(?'file'.*\.ts) *\((?'lineNumber'\d+),(?'columnNumber'\d+)\) *: *(?'message'.*)$");

        public CompileTypeScriptFiles() {
            ChildProcessTimeout = 60000;
        }

        public string CompilerPath { get; set; }
        public string CompilerOptions { get; set; }
        public bool OverwriteReadOnlyFiles { get; set; }
        public int ChildProcessTimeout { get; set; }

        [Required]
        public ITaskItem[] InputFiles { get; set; }

        [Output]
        public ITaskItem[] OutputFiles { get; set; }

        public override bool Execute() {
            var compilerPath = CompilerPath ?? "tsc";
            var compilerOptions = CompilerOptions ?? string.Empty;
            var inputFiles = ReferenceOrderer.OrderFiles(InputFiles ?? new ITaskItem[0]);
            var outputFiles = inputFiles
                .SelectMany(item => new[] {
                    TaskItemWithNewExtension(item, ".js"),
                    TaskItemWithNewExtension(item, ".js.map")
                })
                .ToArray();
            var success = true;

            var singleOutputFile = GetSingleOutputFile(compilerOptions);
            if (singleOutputFile != null)
                outputFiles = new ITaskItem[] {
                    new TaskItem(singleOutputFile),
                    new TaskItem(singleOutputFile + ".map")
                };

            if (OverwriteReadOnlyFiles) {
                var readOnlyFiles =
                    from item in outputFiles
                    let file = new FileInfo(item.ItemSpec)
                    where file.Exists && file.IsReadOnly
                    select file;

                foreach (var file in readOnlyFiles) {
                    file.IsReadOnly = false;
                }
            }

            if (outputFiles.Length > 0) {
                var startInfo =
                    new ProcessStartInfo {
                        FileName = compilerPath,
                        Arguments = string.Join(" ", new[] {compilerOptions}.Concat(inputFiles.Select(item => Quote(item.ItemSpec)))),
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };

                var process = Process.Start(startInfo);
                if (process == null) {
                    Log.LogError("TypeScript process failed to start.");
                    return false;
                }

                var processOutput = new ProcessOutput(process);

                if (!process.WaitForExit(ChildProcessTimeout)) {
                    Log.LogError("Timeout waiting for child process to finish.");
                    success = false;
                }

                if (success && process.ExitCode != 0) {
                    success = false;

                    foreach (var line in processOutput.Lines) {
                        var errorLineMatch = errorLineMatcher.Match(line);

                        if (errorLineMatch.Success) {
                            var file = new FileInfo(errorLineMatch.Groups["file"].Value).FullName;
                            var lineNumber = int.Parse(errorLineMatch.Groups["lineNumber"].Value);
                            var columnNumber = int.Parse(errorLineMatch.Groups["columnNumber"].Value) + 1;
                            var message = errorLineMatch.Groups["message"].Value;

                            Log.LogError(null, null, null, file, lineNumber, columnNumber, 0, 0, message);
                        }
                        else
                            Log.LogWarning(line);
                    }

                    Log.LogError("Typescript compiler exited with code " + process.ExitCode);
                }
            }

            OutputFiles = outputFiles.Where(item => File.Exists(item.ItemSpec)).ToArray();
            return success;
        }

        private static string GetSingleOutputFile(string compilerOptions) {
            var singleOutputFileMatch = Regex.Match(compilerOptions, @"--out\s+(?'file'(""[^""]*"")|([^\s]*))");

            if (!singleOutputFileMatch.Success)
                return null;

            var singleOutputFile = singleOutputFileMatch.Groups["file"].Value;

            if (singleOutputFile.StartsWith("\"") && singleOutputFile.EndsWith("\""))
                singleOutputFile = singleOutputFile.Substring(1, singleOutputFile.Length - 2);

            return singleOutputFile;
        }

        private static string Quote(string s) {
            return '"' + s + '"';
        }

        private static ITaskItem TaskItemWithNewExtension(ITaskItem item, string extension) {
            var newItem = new TaskItem(Path.ChangeExtension(item.ItemSpec, extension));

            item.CopyMetadataTo(newItem);

            return newItem;
        }

        private class ProcessOutput {
            private readonly List<string> _lines = new List<string>();

            public ProcessOutput(Process process) {
                process.ErrorDataReceived += (sender, e) => DataReceived(e.Data);
                process.BeginErrorReadLine();

                process.OutputDataReceived += (sender, e) => DataReceived(e.Data);
                process.BeginOutputReadLine();
            }

            private void DataReceived(string data) {
                if (data == null)
                    return;

                lock (_lines) {
                    _lines.Add(data);
                }
            }

            public IEnumerable<string> Lines {
                get {
                    lock (_lines) {
                        return _lines.ToArray();
                    }
                }
            }


        }

        private static class ReferenceOrderer {
            private static readonly Regex referencePattern = new Regex(@"^\s*///\s*<reference.*path=('|"")(?'path'.*)(\1)\s*/>\s*$", RegexOptions.Multiline);

            public static ITaskItem[] OrderFiles(IEnumerable<ITaskItem> inputFiles) {
                var fileLookup = inputFiles.ToDictionary(ToReference);
                var orderedFileReferences = new List<string>();

                foreach (var fileReference in fileLookup.Keys)
                    AddFileAndReferencedFiles(fileReference, orderedFileReferences, fileLookup, Enumerable.Empty<string>());

                return orderedFileReferences.Select(f => fileLookup[f]).ToArray();
            }

            private static void AddFileAndReferencedFiles(string fileReference, ICollection<string> orderedFileReferences, IDictionary<string, ITaskItem> fileLookup, IEnumerable<string> callStack) {
                if (orderedFileReferences.Contains(fileReference))
                    return;

                var callStackArray = callStack as IList<string> ?? callStack.ToList();
                var callStackForNested = callStackArray.Concat(new[] {fileReference}).ToList();

                if (callStackArray.Contains(fileReference)) {
                    Trace.WriteLine(string.Format("Reference loop detected '{0}'", string.Join("', '", callStackForNested)));
                    return;
                }

                foreach (var nestedFileReference in ReadReferences(fileLookup[fileReference]).Where(fileLookup.ContainsKey)) {
                    AddFileAndReferencedFiles(nestedFileReference, orderedFileReferences, fileLookup, callStackForNested);
                }

                orderedFileReferences.Add(fileReference);
            }


            private static IEnumerable<string> ReadReferences(ITaskItem inputFile) {
                var file = new FileInfo(inputFile.ItemSpec);
                if (file.Directory == null) return new string[] {};

                return
                    referencePattern
                        .Matches(File.ReadAllText(file.FullName))
                        .Cast<Match>()
                        .Select(match => file.Directory != null
                            ? new FileInfo(Path.Combine(file.Directory.FullName, match.Groups["path"].Value))
                            : null)
                        .Select(ToReference);
            }

            private static string ToReference(ITaskItem inputFile) {
                return ToReference(new FileInfo(inputFile.ItemSpec));
            }

            private static string ToReference(FileInfo file) {
                return Path.ChangeExtension(file.FullName, null);
            }
        }
    }
}