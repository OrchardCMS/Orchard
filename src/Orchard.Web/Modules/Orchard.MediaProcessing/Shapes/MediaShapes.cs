using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Environment;
using Orchard.Forms.Services;
using Orchard.Logging;
using Orchard.MediaProcessing.Models;
using Orchard.MediaProcessing.Services;

namespace Orchard.MediaProcessing.Shapes {
    
    public class MediaShapes : IDependency {
        private readonly Work<IImageProfileManager> _imageProfileManager;

        public MediaShapes(Work<IImageProfileManager> imageProfileManager) {
            _imageProfileManager = imageProfileManager;
        }

        public ILogger Logger { get; set; }

        [Shape]
        public void ResizeMediaUrl(dynamic Shape, dynamic Display, TextWriter Output, ContentItem ContentItem, string Path, int Width, int Height, string Mode, string Alignment, string PadColor) {
            var state = new Dictionary<string, string> {
                {"Width", Width.ToString(CultureInfo.InvariantCulture)},
                {"Height", Height.ToString(CultureInfo.InvariantCulture)},
                {"Mode", Mode},
                {"Alignment", Alignment},
                {"PadColor", PadColor},
            };

            var filter = new FilterRecord {
                Category = "Transform",
                Type = "Resize",
                State = FormParametersHelper.ToString(state)
            };

            var profile = "Transform_Resize"
                + "_w_" + Convert.ToString(Width) 
                + "_h_" + Convert.ToString(Height) 
                + "_m_" + Convert.ToString(Mode)
                + "_a_" + Convert.ToString(Alignment) 
                + "_c_" + Convert.ToString(PadColor);

            MediaUrl(Shape, Display, Output, profile, Path, ContentItem, filter);
        }

        [Shape]
        public void MediaUrl(dynamic Shape, dynamic Display, TextWriter Output, string Profile, string Path, ContentItem ContentItem, FilterRecord CustomFilter) {
            try {
                Shape.IgnoreShapeTracer = true;
                Output.Write(_imageProfileManager.Value.GetImageProfileUrl(Path, Profile, CustomFilter, ContentItem));
            }
            catch (Exception ex) {
                Logger.Error(ex, "An error occured while rendering shape {0} for image {1}", Profile, Path);
            }
        }

    }
}