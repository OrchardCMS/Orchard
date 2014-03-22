using System.Collections.Generic;

namespace Orchard.Azure.MediaServices.ViewModels.Settings {
    public class EncodingSettingsViewModel {
        public IEnumerable<string> WamsEncodingPresets { get; set; }
        public int DefaultWamsEncodingPresetIndex { get; set; }
    }
}