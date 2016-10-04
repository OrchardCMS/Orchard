﻿using System.Xml.Linq;

namespace Orchard.MediaLibrary.ViewModels {
    public class OEmbedViewModel {
        public string FolderPath { get; set; }
        public string Url { get; set; }
        public XDocument Content { get; set; }
        public int? ReplaceId { get; set; }
    }
}