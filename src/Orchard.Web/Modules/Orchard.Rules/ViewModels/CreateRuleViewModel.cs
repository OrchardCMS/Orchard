using System.ComponentModel.DataAnnotations;

namespace Orchard.Rules.ViewModels {
    public class CreateRuleViewModel {
        [Required, StringLength(1024)]
        public string Name { get; set; }

    }
}