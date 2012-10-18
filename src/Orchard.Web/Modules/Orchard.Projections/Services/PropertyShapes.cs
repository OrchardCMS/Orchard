using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Projections.Models;
using Orchard.Tokens;
using Orchard.Utility.Extensions;

namespace Orchard.Projections.Services {
    public class PropertyShapes : IDependency {
        private readonly ITokenizer _tokenizer;

        public PropertyShapes(ITokenizer tokenizer) {
            _tokenizer = tokenizer;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [Shape]
        public void Properties(dynamic Display, TextWriter Output, HtmlHelper Html, IEnumerable<dynamic> Items) {
            foreach (var item in Items) {
                if((bool)item.Property.ExcludeFromDisplay) {
                    continue;
                }

                Output.WriteLine(Display(item));
            }
        }

        [Shape] 
        public void LayoutGroup(dynamic Display, TextWriter Output, HtmlHelper Html, dynamic Key, dynamic List) {
            Output.WriteLine(Display(Key)); 
            Output.WriteLine(Display(List));
        }

        [Shape]
        public void PropertyWrapper(
            dynamic Display, 
            TextWriter Output, 
            HtmlHelper Html,
            UrlHelper Url, 
            dynamic Item,
            ContentItem ContentItem,
            ContentItemMetadata ContentItemMetadata,
            PropertyRecord Property
            ) {

            // Display will encode any string which is not IHtmlString
            string resultOutput = Convert.ToString(Display(Item));
            var resultIsEmpty = String.IsNullOrEmpty(resultOutput) || (resultOutput == "0" && Property.ZeroIsEmpty);

            if(Property.HideEmpty && resultIsEmpty) {
                return;
            }

            if(Property.RewriteOutput) {
                resultOutput = _tokenizer.Replace(Property.RewriteText, new Dictionary<string, object> { { "Text", resultOutput }, { "Content", ContentItem } });
            }

            if(Property.StripHtmlTags) {
                resultOutput = resultOutput.RemoveTags();
            }

            if(Property.TrimLength) {
                var ellipsis = Property.AddEllipsis ? "&#160;&#8230;" : "";
                resultOutput = resultOutput.Ellipsize(Property.MaxLength, ellipsis, Property.TrimOnWordBoundary);
            }

            if(Property.TrimWhiteSpace) {
                resultOutput = resultOutput.Trim();
            }

            if(Property.PreserveLines) {
                using(var sw = new StringWriter()) {
                    using(var sr = new StringReader(resultOutput)) {
                        string line;
                        while(null != (line = sr.ReadLine())) {
                            sw.WriteLine(line);
                            sw.WriteLine("<br />");
                        }
                    }
                    resultOutput = sw.ToString();
                }
            }

            var wrapperTag = new TagBuilder(Property.CustomizeWrapperHtml && !String.IsNullOrEmpty(Property.CustomWrapperTag) ? Property.CustomWrapperTag : "div");
            
            if (Property.CustomizeWrapperHtml && !String.IsNullOrEmpty(Property.CustomWrapperCss)) {
                wrapperTag.AddCssClass(_tokenizer.Replace(Property.CustomWrapperCss, new Dictionary<string, object>()));
            }

            if (!(Property.CustomizeWrapperHtml && Property.CustomWrapperTag == "-")) {
                Output.Write(wrapperTag.ToString(TagRenderMode.StartTag));
            }

            if (Property.CreateLabel) {
                var labelTag = new TagBuilder(Property.CustomizeLabelHtml && !String.IsNullOrEmpty(Property.CustomLabelTag) ? Property.CustomLabelTag : "span");

                if (Property.CustomizeLabelHtml && !String.IsNullOrEmpty(Property.CustomLabelCss)) {
                    labelTag.AddCssClass(_tokenizer.Replace(Property.CustomLabelCss, new Dictionary<string, object>()));
                }

                if (!(Property.CustomizeLabelHtml && Property.CustomLabelTag == "-")) {
                    Output.Write(labelTag.ToString(TagRenderMode.StartTag));
                }

                Output.Write(_tokenizer.Replace(Property.Label, new Dictionary<string, object>()));

                if (!(Property.CustomizeLabelHtml && Property.CustomLabelTag == "-")) {
                    Output.Write(labelTag.ToString(TagRenderMode.EndTag));
                } 
            }

            var propertyTag = new TagBuilder(Property.CustomizePropertyHtml && !String.IsNullOrEmpty(Property.CustomPropertyTag) ? Property.CustomPropertyTag : "span");
            
            if (Property.CustomizePropertyHtml && !String.IsNullOrEmpty(Property.CustomPropertyCss)) {
                propertyTag.AddCssClass(_tokenizer.Replace(Property.CustomPropertyCss, new Dictionary<string, object>()));
            }

            if (!(Property.CustomizePropertyHtml && Property.CustomPropertyTag == "-")) {
                Output.Write(propertyTag.ToString(TagRenderMode.StartTag));
            }

            if (!resultIsEmpty) {
                if (Property.LinkToContent) {
                    var linkTag = new TagBuilder("a");
                    linkTag.Attributes.Add("href", Url.RouteUrl(ContentItemMetadata.DisplayRouteValues));
                    linkTag.InnerHtml = resultOutput;
                    Output.Write(linkTag.ToString());
                }
                else {
                    Output.Write(resultOutput);
                }
            }
            else {
                Output.Write(_tokenizer.Replace(Property.NoResultText, new Dictionary<string, object>()));
            }

            if (!(Property.CustomizePropertyHtml && Property.CustomPropertyTag == "-")) {
                Output.Write(propertyTag.ToString(TagRenderMode.EndTag));
            }

            if (!(Property.CustomizeWrapperHtml && Property.CustomWrapperTag == "-")) {
                Output.Write(wrapperTag.ToString(TagRenderMode.EndTag));
            }
        }
    }
}
