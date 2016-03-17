using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Settings;
using Orchard.Services;

namespace Orchard.Core.Feeds.StandardBuilders {
    public class ItemInspector {
        private readonly IContent _item;
        private readonly ContentItemMetadata _metadata;
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;
        private readonly ICommonPart _common;
        private readonly ITitleAspect _titleAspect;
        private readonly BodyPart _body;

        public ItemInspector(IContent item, ContentItemMetadata metadata) : this(item, metadata, Enumerable.Empty<IHtmlFilter>()) {}

        public ItemInspector(IContent item, ContentItemMetadata metadata, IEnumerable<IHtmlFilter> htmlFilters) {
            _item = item;
            _metadata = metadata;
            _htmlFilters = htmlFilters;
            _common = item.Get<ICommonPart>();
            _titleAspect = item.Get<ITitleAspect>();
            _body = item.Get<BodyPart>();
        }

        public string Title {
            get {
                if (_metadata != null && !string.IsNullOrEmpty(_metadata.DisplayText))
                    return _metadata.DisplayText;
                if (_titleAspect != null && !string.IsNullOrEmpty(_titleAspect.Title))
                    return _titleAspect.Title;
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
                    return _htmlFilters.Aggregate(_body.Text, (text, filter) => filter.ProcessContent(text, GetFlavor(_body)));
                }
                return Title;
            }
        }

        public DateTime? PublishedUtc {
            get {
                if (_common != null && _common.CreatedUtc != null)
                    return _common.CreatedUtc;
                return null;
            }
        }

        private static string GetFlavor(BodyPart part) {
            var typePartSettings = part.Settings.GetModel<BodyTypePartSettings>();
            return (typePartSettings != null && !string.IsNullOrWhiteSpace(typePartSettings.Flavor))
                       ? typePartSettings.Flavor
                       : part.PartDefinition.Settings.GetModel<BodyPartSettings>().FlavorDefault;
        }
    }
}