using System.Collections.Generic;
using Orchard.Mvc.ViewModels;

namespace Orchard.MetaData.ViewModels
{
    public class ContentTypesIndexViewModel : BaseViewModel {
        public IList<ContentTypeEntry> ContentTypes { get; set; }
        public IList<ContentTypePartEntry> ContentTypeParts { get; set; }
        public ContentTypesIndexViewModel() {
            ContentTypes=new List<ContentTypeEntry>();
            ContentTypeParts = new List<ContentTypePartEntry>();
        }
        public ContentTypeEntry SelectedContentType { get; set; }
    }

    public class ContentTypeEntry {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public IList<ContentTypePartEntry> ContentTypeParts { get; set; }
        public ContentTypeEntry(){
            ContentTypeParts = new List<ContentTypePartEntry>();
        }
    }

    public class ContentTypePartEntry {
        public string Name { get; set; }
        public bool Selected { get; set; }
    }

    
}
