using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;

namespace Orchard.MediaProcessing.Models {
    public class MediaHtmlFilterSettingsPart : ContentPart {

        public int DensityThreshold {
            get { return this.Retrieve(x => x.DensityThreshold, 2); }
            set { this.Store(x => x.DensityThreshold, value); }
        }

        public int Quality {
            get { return this.Retrieve(x => x.Quality, 95); }
            set { this.Store(x => x.Quality, value); }
        }

        public bool PopulateAlt {
            get { return this.Retrieve(x => x.PopulateAlt, true); }
            set { this.Store(x => x.PopulateAlt, value); }
        }

    }
}