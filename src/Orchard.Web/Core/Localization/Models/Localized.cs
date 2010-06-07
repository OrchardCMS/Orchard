using System.Web.Mvc;
using Orchard.ContentManagement;

namespace Orchard.Core.Localization.Models {
    public sealed class Localized : ContentPart<LocalizedRecord> {
        [HiddenInput(DisplayValue = false)]
        public int Id { get { return ContentItem.Id; } }

        public int CultureId {
            get { return Record.CultureId; }
            set { Record.CultureId = value; }
        }

        public int MasterContentItemId {
            get { return Record.MasterContentItemId; }
            set { Record.MasterContentItemId = value; }
        }

    }
}
