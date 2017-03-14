using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Configuration;

namespace Orchard.MediaLibrary.ViewModels {
    public class MediaManagerFolderEditViewModel {
        [Required]
        public string Name { get; set; }
        public string FolderPath { get; set; }
        public string InvalidCharactersPattern {
            get {
                return Models.MediaPart.InvalidNameCharactersPattern.Replace(@"\", @"\\");
            }
        }
    }
}
