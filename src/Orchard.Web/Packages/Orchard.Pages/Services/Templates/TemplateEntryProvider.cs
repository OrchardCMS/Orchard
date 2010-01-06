using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;

namespace Orchard.Pages.Services.Templates {
    public interface ITemplateEntryProvider : IDependency {
        IEnumerable<TemplateEntry> List();
    }

    public class TemplateEntryProvider : ITemplateEntryProvider {
        public IEnumerable<TemplateEntry> List() {
            //TODO: Figure out a way to formalize the notion of "Orchard Package View Folder"
            const string templatesVirtualPath = "~/Packages/Orchard.Pages/Views/Templates";

            VirtualPathProvider vpathProvider = HostingEnvironment.VirtualPathProvider;
            VirtualDirectory templatesDirectory = vpathProvider.GetDirectory(templatesVirtualPath);
            foreach (VirtualFile file in templatesDirectory.Files) {
                if (file.Name.EndsWith(".aspx")){
                    yield return new TemplateEntry { Name = RemoveFileExtension(file.Name), Content = new StreamReader(file.Open()) };
                }
            }
        }

        private static string RemoveFileExtension(string fileName) {
            int lastIndex = fileName.LastIndexOf('.');
            if (lastIndex == -1) {
                return fileName;
            }
            return fileName.Substring(0, lastIndex);
        }
    }
}
