using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuild.Orchard.Tasks {
    public class StageProjectAlteration : Task {
        public string ProjectFileName { get; set; }

        public ITaskItem[] AddContentFiles { get; set; }

        [Output]
        public ITaskItem[] ExtraFiles { get; set; }



        public override bool Execute() {
            Log.LogMessage("Altering \"{0}\"", ProjectFileName);

            var context = new Context(this);
            if (context.LoadProject() &&
                context.ChangeProjectReferencesToFileReferences() &&
                context.ChangeLibraryReferencesToFileReferences() &&
                context.FindExtraFiles() &&
                context.AddContentFiles() &&
                context.SaveProject()) {

                Log.LogMessage("Stage project altered successfully");
                return true;
            }

            Log.LogWarning("Stage project alteration failed");
            return false;
        }

        class Context {
            private readonly StageProjectAlteration _task;
            XDocument _document;

            private const string Xmlns = "http://schemas.microsoft.com/developer/msbuild/2003";
            private static readonly XName Project = XName.Get("Project", Xmlns);
            private static readonly XName ItemGroup = XName.Get("ItemGroup", Xmlns);
            private static readonly XName ProjectReference = XName.Get("ProjectReference", Xmlns);
            private static readonly XName Reference = XName.Get("Reference", Xmlns);
            private static readonly XName Name = XName.Get("Name", Xmlns);
            private static readonly XName Include = XName.Get("Include");
            private static readonly XName HintPath = XName.Get("HintPath", Xmlns);
            private static readonly XName SpecificVersion = XName.Get("SpecificVersion", Xmlns);
            private static readonly XName Content = XName.Get("Content", Xmlns);
            private static readonly XName Compile = XName.Get("Compile", Xmlns);
            private static readonly XName None = XName.Get("None", Xmlns);

            public Context(StageProjectAlteration task) {
                _task = task;
            }

            public bool LoadProject() {
                try {
                    _document = XDocument.Load(_task.ProjectFileName);
                    return true;
                }
                catch (Exception) {
                    _task.Log.LogError("Unable to load project file");
                    return false;
                }
            }

            public bool SaveProject() {
                _document.Save(_task.ProjectFileName);
                return true;
            }

            public bool ChangeProjectReferencesToFileReferences() {
                var projectReferences = _document
                    .Elements(Project)
                    .Elements(ItemGroup)
                    .Elements(ProjectReference);

                var referenceItemGroup = _document
                    .Elements(Project)
                    .Elements(ItemGroup)
                    .FirstOrDefault(elt => elt.Elements(Reference).Any());

                if (referenceItemGroup == null) {
                    referenceItemGroup = new XElement(ItemGroup);
                    _document.Root.Add(referenceItemGroup);
                }

                foreach (var projectReferenceName in projectReferences.Elements(Name)) {
                    string oldHintPath = (projectReferenceName.Parent.Element(HintPath) ?? new XElement(HintPath)).Value;
                    string newHintPath = string.Format("bin\\{0}.dll", (string)projectReferenceName);
                    var reference = new XElement(
                        Reference,
                        new XAttribute(Include, (string)projectReferenceName),
                        new XElement(SpecificVersion, "False"),
                        new XElement(HintPath, newHintPath));
                    referenceItemGroup.Add(reference);

                    _task.Log.LogMessage("Project reference \"{0}\": HintPath changed from \"{1}\" to \"{2}\"",
                        (string)projectReferenceName, oldHintPath, newHintPath);
                }

                foreach (var projectReference in projectReferences.ToArray()) {
                    projectReference.Remove();
                }

                return true;
            }

            public bool ChangeLibraryReferencesToFileReferences() {
                var libraryReferences = _document
                    .Elements(Project)
                    .Elements(ItemGroup)
                    .Elements(Reference);

                var referenceItemGroup = _document
                    .Elements(Project)
                    .Elements(ItemGroup)
                    .FirstOrDefault(elt => elt.Elements(Reference).Any());

                if (referenceItemGroup == null) {
                    referenceItemGroup = new XElement(ItemGroup);
                    _document.Root.Add(referenceItemGroup);
                }

                List<XElement> elementsToRemove = new List<XElement>();
                foreach (var hintPathElement in libraryReferences.Elements(HintPath)) {
                    string oldHintPath = hintPathElement.Value;

                    if (!oldHintPath.StartsWith("..\\..\\lib\\"))
                        continue;

                    elementsToRemove.Add(hintPathElement.Parent);
                    // Need to change the hint path from
                    // ..\\..\\lib\\<libraryfolder>\\<AssemblyName>.dll
                    // to
                    // bin\\<AssemblyName>.dll
                    string assemblyFileName = Path.GetFileName(oldHintPath);
                    string newHintPath = Path.Combine("bin", assemblyFileName);
                    var reference = new XElement(
                        Reference,
                        new XAttribute(Include, hintPathElement.Parent.Attribute(Include).Value),
                        new XElement(SpecificVersion, "False"),
                        new XElement(HintPath, newHintPath));
                    referenceItemGroup.Add(reference);

                    _task.Log.LogMessage("Assembly (library) Reference \"{0}\": HintPath changed from \"{1}\" to \"{2}\"",
                        hintPathElement.Parent.Attribute(Include).Value, oldHintPath, newHintPath);
                }

                foreach (var reference in elementsToRemove) {
                    reference.Remove();
                }

                return true;
            }

            public bool FindExtraFiles() {
                var extraFiles = _document
                    .Elements(Project)
                    .Elements(ItemGroup)
                    .Elements().Where(elt => elt.Name == Compile || elt.Name == None)
                    .Attributes(Include)
                    .Select(attr => (string)attr);

                _task.ExtraFiles = extraFiles
                    .Select(file => {
                        _task.Log.LogMessage("Detected extra file \"{0}\"", file);
                        var item = new TaskItem(file);
                        item.SetMetadata("RecursiveDir", Path.GetDirectoryName(file));
                        return item;
                    })
                    .ToArray();


                return true;
            }

            public bool AddContentFiles() {
                var existingContent = _document
                    .Elements(Project)
                    .Elements(ItemGroup)
                    .Elements(Content)
                    .Attributes(Include)
                    .Select(attr => (string)attr);

                var contentItemGroup = _document
                    .Elements(Project)
                    .Elements(ItemGroup)
                    .FirstOrDefault(elt => elt.Elements(Content).Any());

                if (contentItemGroup == null) {
                    contentItemGroup = new XElement(ItemGroup);
                    _document.Root.Add(contentItemGroup);
                }

                if (_task.AddContentFiles != null) {
                    foreach (var addContent in _task.AddContentFiles) {
                        if (existingContent.Contains(addContent.ItemSpec)) {
                            // don't add more than once
                            continue;
                        }
                        _task.Log.LogMessage("Adding Content file \"{0}\"", addContent.ItemSpec);

                        var content = new XElement(
                            Content,
                            new XAttribute(Include, addContent.ItemSpec));
                        contentItemGroup.Add(content);
                    }
                }

                return true;
            }
        }
    }
}
