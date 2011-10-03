using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;

namespace Orchard.Core.Title.Models {
    public class TitlePart : ContentPart<TitlePartRecord>, ITitleAspect {
        [Required]
        public string Title {
            get { return Record.Title; }
            set { Record.Title = value; }
        }
    }
}