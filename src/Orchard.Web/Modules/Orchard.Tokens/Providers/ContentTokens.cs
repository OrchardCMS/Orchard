﻿using System;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Common.Fields;
using Orchard.Core.Common.Models;
using Orchard.Localization;
using Orchard.ContentManagement.FieldStorage;
using Orchard.Mvc.Extensions;

namespace Orchard.Tokens.Providers {
    public class ContentTokens : ITokenProvider {
        private readonly IContentManager _contentManager;
        private readonly UrlHelper _urlHelper;

        public ContentTokens(IContentManager contentManager, UrlHelper urlHelper) {
            _contentManager = contentManager;
            _urlHelper = urlHelper;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("Content", T("Content Items"), T("Content Items"))
                .Token("Id", T("Content Id"), T("Numeric primary key value of content."))
                .Token("Author", T("Content Author"), T("Person in charge of the content."), "User")
                .Token("Date", T("Content Date"), T("Date the content was created."), "DateTime")
                .Token("Identity", T("Identity"), T("Identity of the content."))
                .Token("ContentType", T("Content Type"), T("The name of the item Content Type."), "TypeDefinition")
                .Token("DisplayText", T("Display Text"), T("Title of the content."), "Text")
                .Token("DisplayUrl", T("Display Url"), T("Url to display the content."), "Url")
                .Token("EditUrl", T("Edit Url"), T("Url to edit the content."), "Url")
                .Token("Container", T("Container"), T("The container Content Item."), "Content")
                .Token("Body", T("Body"), T("The body text of the content item."), "Text")
                ;

            // Token descriptors for fields
            foreach (var typeDefinition in _contentManager.GetContentTypeDefinitions()) {
                foreach (var typePart in typeDefinition.Parts) {

                    if (!typePart.PartDefinition.Fields.Any()) {
                        continue;
                    }

                    var partContext = context.For("Content");
                    foreach (var partField in typePart.PartDefinition.Fields) {
                        var field = partField;
                        var tokenName = "Fields." + typePart.PartDefinition.Name + "." + field.Name;

                        // the token is chained with the technical name
                        partContext.Token(tokenName, T("{0} {1}", typePart.PartDefinition.Name, field.Name), T("The content of the {0} field.", partField.DisplayName), field.Name);
                    }
                }
            }

            context.For("TextField", T("Text Field"), T("Tokens for Text Fields"))
                .Token("Length", T("Length"), T("The length of the field."));

            context.For("Url", T("Url"), T("Tokens for Urls"))
                .Token("Absolute", T("Absolute"), T("Absolute url."), "Text");

            context.For("TypeDefinition", T("Type Definition"), T("Tokens for Content Types"))
                .Token("Name", T("Name"), T("Name of the content type."))
                .Token("DisplayName", T("Display Name"), T("Display name of the content type."), "Text")
                .Token("Parts", T("Parts"), T("List of the attached part names."))
                .Token("Fields", T("Fields"), T("Fields for each of the attached parts. For example, Fields.Page.Approved."));
        }

        public void Evaluate(EvaluateContext context) {
            context.For<IContent>("Content")
                .Token("Id", content => content != null ? content.Id : 0)
                .Token("Author", AuthorName)
                .Chain("Author", "User", content => content != null ? content.As<ICommonPart>().Owner : null)
                .Token("Date", Date)
                .Chain("Date", "Date", Date)
                .Token("Identity", content => content != null ? _contentManager.GetItemMetadata(content).Identity.ToString() : String.Empty)
                .Token("ContentType", content => content != null ? content.ContentItem.TypeDefinition.DisplayName : String.Empty)
                .Chain("ContentType", "TypeDefinition", content => content != null ? content.ContentItem.TypeDefinition : null)
                .Token("DisplayText", DisplayText)
                .Chain("DisplayText", "Text", DisplayText)
                .Token("DisplayUrl", DisplayUrl)
                .Chain("DisplayUrl", "Url", DisplayUrl)
                .Token("EditUrl", EditUrl)
                .Chain("EditUrl", "Url", EditUrl)
                .Token("Container", content => DisplayText(Container(content)))
                .Chain("Container", "Content", Container)
                .Token("Body", Body)
                .Chain("Body", "Text", Body)
                ;

            if (context.Target == "Content") {
                var forContent = context.For<IContent>("Content");
                // is there a content available in the context ?
                if (forContent != null && forContent.Data != null && forContent.Data.ContentItem != null) {
                    foreach (var typePart in forContent.Data.ContentItem.TypeDefinition.Parts) {
                        var part = typePart;
                        foreach (var partField in typePart.PartDefinition.Fields) {
                            var field = partField;
                            var tokenName = "Fields." + typePart.PartDefinition.Name + "." + partField.Name;
                            forContent.Token(
                                tokenName,
                                content => Convert.ToString(LookupField(content, part.PartDefinition.Name, field.Name).Storage.Get<object>()));
                            forContent.Chain(
                                tokenName,
                                partField.FieldDefinition.Name,
                                content => LookupField(content, part.PartDefinition.Name, field.Name));
                        }
                    }
                }
            }

            context.For<string>("Url")
                   .Token("Absolute", url => _urlHelper.MakeAbsolute(url))
                   .Chain("Absolute", "Text", url => _urlHelper.MakeAbsolute(url))
                ;

            context.For<TextField>("TextField")
                .Token("Length", field => (field.Value ?? "").Length)
                .Token("Text", field => field.Value ?? "")
                .Chain("Text", "Text", field => field.Value ?? "")
                ;

            context.For<ContentTypeDefinition>("TypeDefinition")
                .Token("Name", def => def.Name)
                .Token("DisplayName", def => def.DisplayName)
                .Chain("DisplayName", "Text", def => def.DisplayName)
                .Token("Parts", def => string.Join(", ", def.Parts.Select(x => x.PartDefinition.Name).ToArray()))
                .Token("Fields", def => string.Join(", ", def.Parts.SelectMany(x => x.PartDefinition.Fields.Select(x2 => x2.FieldDefinition.Name + " " + x.PartDefinition.Name + "." + x2.Name)).ToArray()));
        }

        private IHtmlString AuthorName(IContent content) {
            if (content == null) {
                return new HtmlString(String.Empty); // Null content isn't "Anonymous"
            }

            var commonPart = content.As<ICommonPart>();
            var author = commonPart != null ? commonPart.Owner : null;
            // todo: encoding should be done at a higher level automatically and should be configurable via an options param
            // so it can be disabled
            return author == null ? (IHtmlString)T("Anonymous") : new HtmlString(HttpUtility.HtmlEncode(author.UserName));
        }

        private static ContentField LookupField(IContent content, string partName, string fieldName) {
            return content.ContentItem.Parts
                .Where(part => part.PartDefinition.Name == partName)
                .SelectMany(part => part.Fields.Where(field => field.Name == fieldName))
                .SingleOrDefault();
        }

        private IContent Container(IContent content) {
            var commonPart = content.As<ICommonPart>();
            if (commonPart == null) {
                return null;
            }

            return commonPart.Container;
        }

        private string DisplayText(IContent content) {
            if (content == null) {
                return String.Empty;
            }

            return _contentManager.GetItemMetadata(content).DisplayText;
        }

        private object Date(IContent content) {
            return content != null ? content.As<ICommonPart>().CreatedUtc : null;
        }

        private string DisplayUrl(IContent content) {
            if (content == null) {
                return String.Empty;
            }

            return _urlHelper.RouteUrl(_contentManager.GetItemMetadata(content).DisplayRouteValues);
        }

        private string EditUrl(IContent content) {
            if (content == null) {
                return String.Empty;
            }

            return _urlHelper.RouteUrl(_contentManager.GetItemMetadata(content).EditorRouteValues);
        }

        private string Body(IContent content) {
            if (content == null) {
                return String.Empty;
            }

            var bodyPart = content.As<BodyPart>();
            if (bodyPart == null) {
                return String.Empty;
            }

            return bodyPart.Text;
        }
    }
}