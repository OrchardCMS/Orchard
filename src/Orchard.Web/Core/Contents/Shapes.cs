using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.Localization;
using Orchard.Mvc;
using System.Web.Routing;

// ReSharper disable InconsistentNaming

namespace Orchard.Core.Contents {
    public class Shapes : IShapeTableProvider {
        public Shapes() {
            T = NullLocalizer.Instance;
        }

        public IDisplayHelperFactory DisplayHelperFactory { get; set; }

        public Localizer T { get; set; }

        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Content")
                .OnCreated(created => {
                    var content = created.Shape;
                    content.Child.Add(created.New.PlaceChildContent(Source: content));
                })
                .OnDisplaying(displaying => {
                    ContentItem contentItem = displaying.Shape.ContentItem;
                    if (contentItem != null) {
                        // Alternates in order of specificity. 
                        // Display type > content type > specific content > display type for a content type > display type for specific content

                        // Content__[DisplayType] e.g. Content.Summary
                        displaying.ShapeMetadata.Alternates.Add("Content_" + displaying.ShapeMetadata.DisplayType);

                        // Content__[ContentType] e.g. Content-BlogPost
                        displaying.ShapeMetadata.Alternates.Add("Content__" + contentItem.ContentType);

                        // Content__[Id] e.g. Content-42
                        displaying.ShapeMetadata.Alternates.Add("Content__" + contentItem.Id);

                        // Content_[DisplayType]__[ContentType] e.g. Content-BlogPost.Summary
                        displaying.ShapeMetadata.Alternates.Add("Content_" + displaying.ShapeMetadata.DisplayType + "__" + contentItem.ContentType);

                        // Content_[DisplayType]__[Id] e.g. Content-42.Summary
                        displaying.ShapeMetadata.Alternates.Add("Content_" +  displaying.ShapeMetadata.DisplayType + "__" + contentItem.Id);

                        if ( !displaying.ShapeMetadata.DisplayType.Contains("Admin") )
                            displaying.ShapeMetadata.Wrappers.Add("Content_ControlWrapper");
                    }
                });
        }


        [Shape]
        public MvcHtmlString DisplayLink(dynamic Display, HtmlHelper Html, IContent ContentItem, dynamic Value) {
            // This shape renders a link to a Content Item's Display route. The Value may be a nested shape or any string convertable object.
            var metadata = ContentItem.ContentItem.ContentManager.GetItemMetadata(ContentItem);
            if (metadata.DisplayRouteValues == null) {
                return null;
            }
            var content = NonNullOrEmpty((object)Value, metadata.DisplayText, T("view"));

            var displayText = (string)Display(content).ToString();
            return Html.ActionLink(displayText, Convert.ToString(metadata.DisplayRouteValues["action"]), metadata.DisplayRouteValues);
        }

        [Shape]
        public MvcHtmlString EditLink(dynamic Display, HtmlHelper Html, IContent ContentItem, dynamic Value, object ReturnUrl, object RouteValues, bool? AdminLink) {
            // This shape renders a link to a Content Item's Editor route. The Value may be a nested shape or any string convertable object.
            // ReturnUrl may be (boolean)true to use the current RawUrl of the request, or a string to set it specifically.
            var metadata = ContentItem.ContentItem.ContentManager.GetItemMetadata(ContentItem);
            if ((AdminLink.GetValueOrDefault() && metadata.AdminRouteValues == null) || (!AdminLink.GetValueOrDefault() && metadata.EditorRouteValues == null)) {
                return null;
            }
            var content = NonNullOrEmpty((object)Value, metadata.DisplayText, ContentItem.ContentItem.TypeDefinition.DisplayName);

            string returnUrl = null;
            if (ReturnUrl != null) {
                if (ReturnUrl is bool) {
                    if ((bool)ReturnUrl) {
                        returnUrl = Html.ViewContext.HttpContext.Request.RawUrl;
                    }
                }
                else {
                    returnUrl = ReturnUrl.ToString();
                }
            }

            if (returnUrl != null) {
                if (RouteValues == null) {
                    RouteValues = new { ReturnUrl = returnUrl };
                }
                else {
                    RouteValues = MergeRouteValues(new RouteValueDictionary(RouteValues), new RouteValueDictionary(new { ReturnUrl = returnUrl }));
                }
            }

            var displayText = (string)Display(content).ToString();
            var metadataRouteValues = AdminLink.GetValueOrDefault() ? metadata.AdminRouteValues : metadata.EditorRouteValues;
            return Html.ActionLink(displayText,
                Convert.ToString(metadataRouteValues["action"]),
                RouteValues == null ? metadataRouteValues : MergeRouteValues(metadataRouteValues, new RouteValueDictionary(RouteValues)));
        }

        private static object NonNullOrEmpty(params object[] values) {
            foreach (var value in values) {
                if (value != null && (!(value is string) || ((string)value) != "")) {
                    return value;
                }
            }
            return null;
        }

        private static RouteValueDictionary MergeRouteValues(RouteValueDictionary dictionary, IEnumerable<KeyValuePair<string, object>> dictionaryToMerge) {
            if (dictionaryToMerge == null) {
                return dictionary;
            }

            var newDictionary = new RouteValueDictionary(dictionary);
            foreach (var valueDictionary in dictionaryToMerge) {
                newDictionary[valueDictionary.Key] = valueDictionary.Value;
            }

            return newDictionary;
        }

    }
}
