using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.Azure.MediaServices.Infrastructure.Assets;
using Orchard.Azure.MediaServices.Models.Assets;

namespace Orchard.Azure.MediaServices.ViewModels.Media {
    public class AssetViewModel {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IncludeInPlayer { get; set; }
        public string MediaQuery { get; set; }
        public IEnumerable<AssetDriverResult> SpecializedSettings { get; set; }
        public Asset Asset { get; set; }
    }
}