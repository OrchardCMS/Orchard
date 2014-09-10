using System.Linq;

namespace Orchard.ImportExport.Models {
    public class DeploymentMetadata {
        private string _key;
        private string _value;
        public const string ExportStepPrefix = "DeploymentMetadata";

        public string Key {
            get { return _key; }
            set { _key = CleanString(value, ";", ":"); }
        }

        public string Value {
            get { return _value; }
            set { _value = CleanString(value, ";"); }
        }

        public DeploymentMetadata() {}

        public DeploymentMetadata(string key, string value) {
            Key = key;
            Value = value;
        }

        public string ToDisplayString() {
            return string.Format("{0}: {1}", Key, Value);
        }

        public string ToExportStep() {
            return string.Format("{0}{1}: {2}", ExportStepPrefix, Key, Value);
        }

        public static DeploymentMetadata FromDisplayString(string displayString) {
            if (string.IsNullOrWhiteSpace(displayString) || displayString.IndexOf(':') < 0)
                return null;

            var key = displayString.Substring(0, displayString.IndexOf(':')).Trim();
            var value = displayString.Substring(displayString.IndexOf(':') + 1).Trim();

            return !string.IsNullOrEmpty(key) ? new DeploymentMetadata {Key = key, Value = value} : null;
        }

        /// <summary>
        /// Extras a key (SomeKey) and value (Some value) pair from a string e.g. 'DeploymentMetadataSomeKey: Some value'
        /// </summary>
        /// <param name="exportStep"></param>
        /// <returns></returns>
        public static DeploymentMetadata FromExportStep(string exportStep) {
            if (exportStep == null || !exportStep.StartsWith(ExportStepPrefix))
                return null;

            var key = exportStep.Substring(0, exportStep.IndexOf(':')).Replace(ExportStepPrefix, string.Empty).Trim();
            var value = exportStep.Substring(exportStep.IndexOf(':') + 1).Trim();

            return !string.IsNullOrEmpty(key) ? new DeploymentMetadata {Key = key, Value = value} : null;
        }

        private static string CleanString(string input, params string[] illegalStrings) {
            return string.IsNullOrEmpty(input) 
                ? input 
                : illegalStrings.Aggregate(input, (current, illegalString) => current.Replace(illegalString, string.Empty));
        }
    }
}
