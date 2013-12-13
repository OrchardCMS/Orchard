using System.ComponentModel.DataAnnotations;

namespace Orchard.Messaging.ViewModels {
    public class MessageQueueViewModel {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string ReturnUrl { get; set; }
    }
}