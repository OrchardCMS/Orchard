using System;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Core.Routable.Models;

namespace Orchard.Core.Feeds.StandardBuilders {
    public class ItemInspector {
        private readonly IContent _item;
        private readonly ContentItemMetadata _metadata;
        private readonly ICommonPart _common;
        private readonly RoutePart _routable;
        private readonly BodyPart _body;

        public ItemInspector(IContent item, ContentItemMetadata metadata) {
            _item = item;
            _metadata = metadata;
            _common = item.Get<ICommonPart>();
            _routable = item.Get<RoutePart>();
            _body = item.Get<BodyPart>();
        }

        public string Title {
            get {
                if (_metadata != null && !string.IsNullOrEmpty(_metadata.DisplayText))
                    return _metadata.DisplayText;
                if (_routable != null && !string.IsNullOrEmpty(_routable.Title))
                    return _routable.Title;
                return _item.ContentItem.ContentType + " #" + _item.ContentItem.Id;
            }
        }

        public RouteValueDictionary Link {
            get {
                if (_metadata != null) {
                    return _metadata.DisplayRouteValues;
                }
                return null;
            }
        }

        public string Description {
            get {
                if (_body != null && !string.IsNullOrEmpty(_body.Text)) {
                    return _body.Text;
                }
                return Title;
            }
        }

        public DateTime? PublishedUtc {
            get {
                if (_common != null && _common.PublishedUtc != null)
                    return _common.PublishedUtc;
                return null;
            }
        }
    }
}