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

        public string Type {
            get { return _metaData.Type; }
        }

        public string DisplayType {
            get { return _metaData.DisplayType; }
        }

        public string Position {
            get { return _metaData.Position; }
        }

        public string PlacementSource {
            get { return _metaData.PlacementSource; }
        }

        public string Prefix {
            get { return _metaData.Prefix; }
        }

        public IList<string> Wrappers {
            get { return _metaData.Wrappers.Any() ? _metaData.Wrappers : null; }
        }

        public IList<string> Alternates {
            get { return _metaData.Alternates.Any() ? _metaData.Alternates : null; }
        }

        public IList<string> BindingSources {
            get { return _metaData.BindingSources.Any() ? _metaData.BindingSources : null; }
        }

        public TimeSpan Duration { get; set; }
    }
}