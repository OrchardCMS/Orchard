using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;

namespace Orchard.Core.Title.Models {
    public class TitlePart : ContentPart<TitlePartRecord>, ITitleAspect {
        [Required]
        public string Title {
            get {
                return Get("Title");
            }
            set {
                Set("Title", value);
                Record.Title = value;
            }
        }
    }
}