using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuild.Orchard.Tasks {
    public class FileUpdateLines : Task {
        private int _replacementCount = -1;
        private Encoding _encodingValue { get; set; }

        public ITaskItem[] Files { get; set; }

        public string Regex { get; set; }
        public string ReplacementText { get; set; }
        public bool IgnoreCase { get; set; }
        public string Encoding {
            get {
                return this._encodingValue.WebName;
            }
            set {
                this._encodingValue = System.Text.Encoding.GetEncoding(value);
            }
        }

        public FileUpdateLines() {
            _replacementCount = -1;
            _encodingValue = System.Text.Encoding.UTF8;
        }

        public override bool Execute() {
            RegexOptions options = RegexOptions.None;
            options |= RegexOptions.Singleline;
            if (this.IgnoreCase) {
                options |= RegexOptions.IgnoreCase;
            }
            if (this._replacementCount == 0) {
                this._replacementCount = -1;
            }
            Regex regex = new Regex(Regex, options);
            try {
                foreach (ITaskItem item in Files) {
                    string itemSpec = item.ItemSpec;
                    Log.LogMessage("Updating File \"{0}\".", itemSpec);
                    int replacementCount = ProcessLines(itemSpec, regex);
                    Log.LogMessage("  Replaced {0} occurence(s) of \"{1}\" with \"{2}\".", replacementCount, Regex, ReplacementText);
                }
            }
            catch (Exception exception) {
                Log.LogErrorFromException(exception);
                return false;
            }
            return true;
        }

        private int ProcessLines(string itemSpec, Regex regex) {
            int replacementCount = 0;

            using (var writer = new StringWriter()) {
                using (var reader = new StreamReader(itemSpec, _encodingValue)) {
                    for (var line = reader.ReadLine(); line != null; line = reader.ReadLine()) {
                        var newLine = regex.Replace(line, ReplacementText, _replacementCount);
                        if (newLine != line)
                            replacementCount++;
                        writer.WriteLine(newLine);
                    }
                }

                writer.Flush();
                File.WriteAllText(itemSpec, writer.ToString(), _encodingValue);
            }

            return replacementCount;
        }
    }
}
