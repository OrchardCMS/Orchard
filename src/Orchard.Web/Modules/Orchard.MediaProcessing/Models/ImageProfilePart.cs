using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;

namespace Orchard.MediaProcessing.Models {
    public class ImageProfilePart : ContentPart<ImageProfilePartRecord> {

        public string Name {
            get { return Record.Name; }
            set { Record.Name = value; }
        }

        public DateTime? ModifiedUtc {
            get { return this.As<ICommonPart>().ModifiedUtc; }
            set { this.As<ICommonPart>().ModifiedUtc = value; }
        }

        public IList<FilterRecord> Filters {
            get { return Record.Filters; }
        }

        public IList<FileNameRecord> FileNames {
            get { return Record.FileNames; }
        }
    }
}