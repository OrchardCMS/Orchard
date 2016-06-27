using System.Collections.Generic;
using Orchard.Azure.MediaServices.Models;

namespace Orchard.Azure.MediaServices.ViewModels.Settings {
    public class EncodingSettingsViewModel {
        public IEnumerable<EncodingPreset> WamsEncodingPresets { get; set; }
        public int DefaultWamsEncodingPresetIndex { get; set; }
    }
}