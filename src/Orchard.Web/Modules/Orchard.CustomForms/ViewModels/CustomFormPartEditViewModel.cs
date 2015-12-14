using System.Collections.Generic;
using Orchard.CustomForms.Models;

namespace Orchard.CustomForms.ViewModels {
    public class CustomFormPartEditViewModel {
        public IEnumerable<string> ContentTypes { get; set; }
        public CustomFormPart CustomFormPart { get; set; } 
    }
}