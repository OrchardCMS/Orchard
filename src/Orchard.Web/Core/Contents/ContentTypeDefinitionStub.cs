using System.ComponentModel.DataAnnotations;

namespace Orchard.Core.Contents {
    public class ContentTypeDefinitionStub {
        [StringLength(128)]
        public string Name { get; set; }
        [Required, StringLength(1024)]
        public string DisplayName { get; set; }
    }
}