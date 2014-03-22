using System;
using System.ComponentModel.DataAnnotations;

namespace Orchard.Azure.MediaServices.ViewModels.Settings {
    public class GeneralSettingsViewModel {
        public string WamsAccountName { get; set; }
        public string WamsAccountKey { get; set; }
		public string StorageAccountKey { get; set; }
        public bool EnableDynamicPackaging { get; set; }
        [Required]
        public TimeSpan AccessPolicyDuration { get; set; }
        [Required]
        public string AllowedVideoFilenameExtensions { get; set; }
    }
}