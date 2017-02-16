using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Configuration;

namespace Orchard.MediaLibrary.ViewModels {
    public class MediaManagerFolderEditViewModel {
        [Required]
        public string Name { get; set; }
        public string FolderPath { get; set; }
        public string InvalidCharactersPattern {
            get {
                string invalidChars = ((SystemWebSectionGroup)WebConfigurationManager.OpenWebConfiguration(null).GetSectionGroup("system.web")).HttpRuntime.RequestPathInvalidCharacters;
                string[] invalidCharacters = invalidChars.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                string pattern = @"[^/" + string.Join("", invalidCharacters) + @"]+";
                return pattern.Replace(@"\", @"\\");
            }
        }
    }
}
