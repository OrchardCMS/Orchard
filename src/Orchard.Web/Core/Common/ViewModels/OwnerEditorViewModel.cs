using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Orchard.Core.Common.ViewModels {
    public class OwnerEditorViewModel {
        [Required]
        public string Owner { get; set; }
    }
}
