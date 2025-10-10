using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.Services;
using Orchard.ContentTypes.Settings;

namespace Orchard.ContentTypes.ViewModels {
    public class EditPlacementViewModel {
        public ContentTypeDefinition ContentTypeDefinition { get; set; }
        public List<DriverResultPlacement> AllPlacements { get; set; }
        public List<Tab> Tabs { get; set; }
        public Tab Content { get; set; }
    }

    public class Tab {
        public string Name { get; set; }
        public List<Card> Cards { get; set; }
    }

    public class Card {
        public string Name { get; set; }
        public string TabName { get; set; }
        public List<DriverResultPlacement> Placements { get; set; }

    }
}