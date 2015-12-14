using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.UI.Navigation;

namespace Orchard.MediaLibrary.ViewModels {
    public class MediaManagerImportViewModel {
        public IEnumerable<MenuItem> Menu { get; set; }
        public IEnumerable<string> ImageSets { get; set; }
        public string FolderPath { get; set; }
        public IEnumerable<ContentTypeDefinition> MediaTypes { get; set; }
    }
}