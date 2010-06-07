using System.Web.Mvc;
using Orchard.ContentManagement;

namespace Orchard.Core.Localization.Models {
    public sealed class Localized : ContentPart<LocalizedRecord> {
        [HiddenInput(DisplayValue = false)]
        public int Id { get { return ContentItem.Id; } }
    }
}
