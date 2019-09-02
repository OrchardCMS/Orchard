using System;
using System.Collections.Generic;
using System.Linq;
using Glimpse.Core.Message;
using Orchard.DisplayManagement.Shapes;
using Orchard.Glimpse.Models;

namespace Orchard.Glimpse.Tabs.Shapes {
    public class ShapeMessage : MessageBase, IDurationMessage {
        private readonly ShapeMetadata _metaData;

        public ShapeMessage(ShapeMetadata metaData) {
            _metaData = metaData;
        }

        public string BindingName { get; set; }
        public string BindingSource { get; set; }

        public string Type => _metaData.Type;

        public string DisplayType => _metaData.DisplayType;

        public string Position => _metaData.Position;

        public string PlacementSource => _metaData.PlacementSource;

        public string Prefix => _metaData.Prefix;

        public IList<string> Wrappers => _metaData.Wrappers.Any() ? _metaData.Wrappers : null;

        public IList<string> Alternates => _metaData.Alternates.Any() ? _metaData.Alternates : null;

        public IList<string> BindingSources => _metaData.BindingSources.Any() ? _metaData.BindingSources : null;

        public TimeSpan Duration { get; set; }
    }
}