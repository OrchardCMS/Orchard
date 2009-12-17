using System;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;

namespace Orchard.Mvc.Html {
    public class FileRegistrationContext : RequestContext {
        public FileRegistrationContext(ViewContext viewContext, IViewDataContainer viewDataContainer, string fileName)
            : base(viewContext.HttpContext, viewContext.RouteData) {
            TemplateControl container = viewDataContainer as TemplateControl;

            if (container != null)
                ContainerVirtualPath = container.AppRelativeVirtualPath.Substring(0,
                                                                                  container.AppRelativeVirtualPath.
                                                                                      IndexOf("/Views",
                                                                                              StringComparison.
                                                                                                  InvariantCultureIgnoreCase));

            FileName = fileName;
        }

        public TemplateControl Container { get; set; }
        public string ContainerVirtualPath { get; set; }
        public string FileName { get; set; }

        public override bool Equals(object obj)
        {
            FileRegistrationContext incoming = obj as FileRegistrationContext;

            return incoming != null &&
                   string.Equals(ContainerVirtualPath, incoming.ContainerVirtualPath,
                                 StringComparison.InvariantCultureIgnoreCase) &&
                   string.Equals(FileName, incoming.FileName, StringComparison.InvariantCultureIgnoreCase);
        }

        internal string GetFilePath(string containerRelativePath) {
            return ContainerVirtualPath.Replace("~/", "/") + containerRelativePath + FileName;
        }
    }
}