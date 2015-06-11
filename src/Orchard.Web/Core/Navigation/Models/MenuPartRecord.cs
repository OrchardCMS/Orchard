using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement.Records;

namespace Orchard.Core.Navigation.Models {
    public class MenuPartRecord : ContentPartRecord {
        public const ushort DefaultMenuTextLength = 255;

        [StringLength(DefaultMenuTextLength)]
        public virtual string MenuText { get; set; }
        public virtual string MenuPosition { get; set; }
        public virtual int MenuId { get; set; }
    }
}