using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.Services;
using Orchard.ContentTypes.Settings;

namespace Orchard.ContentTypes.ViewModels {
    public class EditPlacementViewModel {
        public ContentTypeDefinition ContentTypeDefinition { get; set; }
        public List<DriverResultPlacement> AllPlacements { get; set; }
        public Dictionary<string, List<DriverResultPlacement>> Tabs { get; set; }
        public List<DriverResultPlacement> Content { get; set; }
    }
}