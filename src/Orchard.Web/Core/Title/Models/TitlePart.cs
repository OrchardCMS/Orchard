using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;

namespace Orchard.Core.Title.Models {
    public class TitlePart : ContentPart<TitlePartRecord>, ITitleAspect {
        [Required]
        public string Title {
            get { return this.As<InfosetPart>().Get<TitlePart>("Title"); }
            set {
                this.As<InfosetPart>().Set<TitlePart>("Title", value);
                Record.Title = value;
            }
        }
    }
}