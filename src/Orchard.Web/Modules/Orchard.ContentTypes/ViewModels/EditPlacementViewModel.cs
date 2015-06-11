using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.Services;
using Orchard.ContentTypes.Settings;

namespace Orchard.ContentTypes.ViewModels {
    public class EditPlacementViewModel {
        public ContentTypeDefinition ContentTypeDefinition { get; set; }
        public PlacementSettings[] PlacementSettings { get; set; }
        public List<DriverResultPlacement> AllPlacements { get; set; }
    }
}