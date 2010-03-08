using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;

namespace Orchard.Mvc.Html {
    public class FileRegistrationContext : RequestContext {
        private readonly TagBuilder _tagBuilder;
        private static readonly Dictionary<string, string> _filePathAttributes = new Dictionary<string, string> {{"script", "src"}, {"link", "href"}};

        public FileRegistrationContext(ControllerContext viewContext, IViewDataContainer viewDataContainer, string tagName, string fileName)
            : this(viewContext, viewDataContainer, tagName, _filePathAttributes[tagName], fileName) {
        }

        public FileRegistrationContext(ControllerContext viewContext, IViewDataContainer viewDataContainer, string tagName, string filePathAttributeName, string fileName)
            : base(viewContext.HttpContext, viewContext.RouteData) {
            Container = viewDataContainer as TemplateControl;

            if (Container != null) {
                ContainerVirtualPath = Container.AppRelativeVirtualPath.Substring(
                    0,
                    Container.AppRelativeVirtualPath.IndexOf(
                        "/Views",
                        StringComparison.InvariantCultureIgnoreCase
                        )
                    );
            }

            FileName = fileName;
            FilePathAttributeName = filePathAttributeName;
            _tagBuilder = new TagBuilder(tagName);
        }

        public TemplateControl Container { get; set; }
        public string ContainerVirtualPath { get; set; }
        public string FileName { get; set; }
        public string Condition { get; set; }
        public string FilePathAttributeName { get; set; }
        public IDictionary<string, string> Attributes { get { return _tagBuilder.Attributes; } }

        public void AddAttribute(string name, string value) {
            _tagBuilder.MergeAttribute(name, value);
        }

        public void SetAttribute(string name, string value) {
            _tagBuilder.MergeAttribute(name, value, true);
        }

        internal string GetFilePath(string containerRelativePath) {
            //todo: (heskew) maybe not here but file paths for non-app locations need to be taken into account
            return Container != null
                       ? Container.ResolveUrl(ContainerVirtualPath + containerRelativePath + FileName)
                       : (ContainerVirtualPath + containerRelativePath + FileName);
        }

        internal string GetTag() {
            return _tagBuilder.ToString();
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            if (ReferenceEquals(this, obj)) {
                return true;
            }
            return obj.GetType() == typeof (FileRegistrationContext) && Equals((FileRegistrationContext) obj);
        }

        public bool Equals(FileRegistrationContext other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }
            if (ReferenceEquals(this, other)) {
                return true;
            }
            return Equals(other.Container, Container) && Equals(other.ContainerVirtualPath, ContainerVirtualPath) && Equals(other.FileName, FileName) && Equals(other.Condition, Condition);
        }

        public override int GetHashCode() {
            unchecked {
                var result = (Container != null ? Container.GetHashCode() : 0);
                result = (result*397) ^ (ContainerVirtualPath != null ? ContainerVirtualPath.GetHashCode() : 0);
                result = (result*397) ^ (FileName != null ? FileName.GetHashCode() : 0);
                result = (result*397) ^ (Condition != null ? Condition.GetHashCode() : 0);
                return result;
            }
        }
    }
}