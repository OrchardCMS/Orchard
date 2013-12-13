using System.ComponentModel.DataAnnotations;

namespace Orchard.Messaging.ViewModels {
    public class MessagePriorityViewModel {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string DisplayText { get; set; }
        public int Value { get; set; }
    }
}