using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuild.Orchard.Tasks {
    /// <summary>
    /// Validate various aspect of a set of Module/Theme project files
    /// </summary>
    public class ValidateExtensionProjectFiles : Task {
        public ITaskItem[] Files { get; set; }

        public override bool Execute() {
            bool result = true;

            foreach (var item in Files) {
                try {
                    ValidateFile(item);
                }
                catch (Exception e) {
                    Log.LogError("Error validating project file \"{0}\"", item);
                    Log.LogErrorFromException(e);
                    result = false;
                }
            }
            return result;
        }

        private void ValidateFile(ITaskItem item) {
            Log.LogMessage("Validating \"{0}\"", item);

            var errors = new Validator(item).Validate();

            if (errors.Any()) {
                foreach (var error in errors) {
                    Log.LogError("", "", "", error.FileName, error.LineNumber, error.ColumnNumber, 0, 0, "{0}", error.Message);
                }
            }
            else {
                Log.LogMessage("Project file \"{0}\" is valid", item);
            }
        }

        public class Validator {
            private const string Xmlns = "http://schemas.microsoft.com/developer/msbuild/2003";
            private static readonly XName Project = XName.Get("Project", Xmlns);
            private static readonly XName PropertyGroup = XName.Get("PropertyGroup", Xmlns);
            private static readonly XName ItemGroup = XName.Get("ItemGroup", Xmlns);
            private static readonly XName ProjectTypeGuids = XName.Get("ProjectTypeGuids", Xmlns);
            private static readonly XName OutputPath = XName.Get("OutputPath", Xmlns);
            private static readonly XName None = XName.Get("None", Xmlns);
            private static readonly XName Content = XName.Get("Content", Xmlns);
            private static readonly XName Include = XName.Get("Include"); // No XmlNs: this is an attribute
            private static readonly XName CodeAnalysisRuleSet = XName.Get("CodeAnalysisRuleSet", Xmlns);

            private static readonly Guid[] MvcGuids = new Guid[] {
                new Guid("{F85E285D-A4E0-4152-9332-AB1D724D3325}") /* MVC2 */, 
                new Guid("{E53F8FEA-EAE0-44A6-8774-FFD645390401}") /* MVC3 */
            };

            private readonly ITaskItem _item;
            private readonly List<Error> _validationErrors = new List<Error>();

            public Validator(ITaskItem item) {
                _item = item;
            }

            public IEnumerable<Error> Validate() {
                XDocument document = XDocument.Load(_item.ItemSpec, LoadOptions.SetLineInfo);
                CheckProjectType(document);
                CheckOutputPath(document);
                //CheckCodeAnalysisRuleSet(document);
                CheckContentFiles(document);
                return _validationErrors;
            }

            private void AddValidationError(XElement element, string message) {
                var error = new Error {
                    Message = message,
                    XElement = element,
                    FileName = _item.ItemSpec,
                    LineNumber = (element as IXmlLineInfo).LineNumber,
                    ColumnNumber = (element as IXmlLineInfo).LinePosition,
                };
                _validationErrors.Add(error);
            }

            private void CheckContentFiles(XDocument document) {
                var elements = document
                    .Elements(Project)
                    .Elements(ItemGroup)
                    .Elements(None);

                foreach (var element in elements) {
                    var filePath = (element.Attribute(Include) == null ? null : element.Attribute(Include).Value);
                    bool isValid = IsValidExcludeFile(filePath);
                    if (!isValid) {
                        string message = string.Format(
                            "\"{0}\" element name for include \"{1}\" should be \"{2}\".",
                            element.Name.LocalName, filePath, Content.LocalName);
                        AddValidationError(element, message);
                    }
                }
            }

            private static bool IsValidExcludeFile(string filePath) {
                var validFilenames = new[] { "packages.config" };
                var validExtensions = new[] { ".sass", ".scss", ".less", ".coffee", ".ls", ".ts", ".md", ".docx" };
                if (string.IsNullOrEmpty(filePath)) return true;

                var fileExtension = Path.GetExtension(filePath);
                var fileName = Path.GetFileName(filePath);
                if (string.IsNullOrEmpty(fileExtension)) return false;

                return 
                    validExtensions.Contains(fileExtension, StringComparer.InvariantCultureIgnoreCase) ||
                    validFilenames.Contains(filePath, StringComparer.InvariantCultureIgnoreCase)
                    ;
            }

            private void CheckCodeAnalysisRuleSet(XDocument document) {
                const string orchardbasiccorrectnessRuleset = "OrchardBasicCorrectness.ruleset";

                var elements = document
                    .Elements(Project)
                    .Elements(PropertyGroup)
                    .Elements(CodeAnalysisRuleSet);

                foreach (var element in elements) {
                    var filename = Path.GetFileName(element.Value);
                    bool isValid = StringComparer.OrdinalIgnoreCase.Equals(filename, orchardbasiccorrectnessRuleset);
                    if (!isValid) {
                        string message = string.Format(
                            "\"{0}\" element should be \"{1}\" instead of \"{2}\".",
                            element.Name.LocalName, orchardbasiccorrectnessRuleset, element.Value);
                        AddValidationError(element, message);
                    }
                }
            }

            private void CheckOutputPath(XDocument document) {
                var elements = document
                    .Elements(Project)
                    .Elements(PropertyGroup)
                    .Elements(OutputPath);

                foreach (var element in elements) {
                    bool isValid =
                        StringComparer.OrdinalIgnoreCase.Equals(element.Value, "bin") ||
                        StringComparer.OrdinalIgnoreCase.Equals(element.Value, "bin\\");

                    if (!isValid) {
                        string message = string.Format(
                            "\"{0}\" element should be \"bin\\\" instead of \"{1}\".",
                            element.Name.LocalName, element.Value);
                        AddValidationError(element, message);
                    }
                }
            }

            private void CheckProjectType(XDocument document) {
                var elements = document
                    .Elements(Project)
                    .Elements(PropertyGroup)
                    .Elements(ProjectTypeGuids);
                foreach (var element in elements) {
                    var guids = element.Value.Split(new char[] { ';' }).Select(g => Guid.Parse(g));

                    foreach (var guid in guids) {
                        if (MvcGuids.Contains(guid)) {
                            string message = string.Format(
                                "\"{0}\" element contains an MVC tooling Guid. " +
                                " This prevents the project from loading in Visual Studio if MVC tooling is not installed.",
                                ProjectTypeGuids.LocalName);
                            AddValidationError(element, message);
                        }
                    }
                }
            }

            public class Error {
                public string Message { get; set; }
                public XElement XElement { get; set; }
                public string FileName { get; set; }
                public int LineNumber { get; set; }
                public int ColumnNumber { get; set; }
            }
        }
    }
}
