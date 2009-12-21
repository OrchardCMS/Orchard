using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Orchard.Mvc.ViewModels;

namespace Orchard.Core.Themes.ViewModels {
    public class CreateThemeViewModel : AdminViewModel {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Author { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Version { get; set; }
        [Required]
        public string Tags { get; set; }
        [Required]
        public string Homepage { get; set; }
    }
}
