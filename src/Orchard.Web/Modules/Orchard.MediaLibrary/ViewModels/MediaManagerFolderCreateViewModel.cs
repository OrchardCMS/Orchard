﻿using System;
using System.Collections.Generic;
using System.Web.Configuration;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.ViewModels {
    public class MediaManagerFolderCreateViewModel {
        public string Name { get; set; }
        public string FolderPath { get; set; }
        public IEnumerable<IMediaFolder> Hierarchy { get; set; }
        public string InvalidCharactersPattern {
            get {
                return Models.MediaPart.InvalidNameCharactersPattern.Replace(@"\", @"\\");
            }
        }
    }
}
