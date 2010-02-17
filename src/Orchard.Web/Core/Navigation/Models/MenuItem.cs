using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Records;

namespace Orchard.Core.Navigation.Models {
    public class MenuItem : ContentPart<MenuItemRecord> {
        [HiddenInput(DisplayValue = false)]
        public int Id { get { return ContentItem.Id; } }

        [Required]
        public string Url {
            get { return Record.Url; }
            set { Record.Url = value; }
        }
    }
}
