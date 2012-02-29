using System;
using System.Xml;
using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Orchard.Comments.Drivers {
    [UsedImplicitly]
    public class CommentPartDriver : ContentPartDriver<CommentPart> {
        private readonly IContentManager _contentManager;
        protected override string Prefix { get { return "Comments"; } }

        public CommentPartDriver(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        protected override void Importing(CommentPart part, ContentManagement.Handlers.ImportContentContext context) {
            var author = context.Attribute(part.PartDefinition.Name, "Author");
            if (author != null) {
                part.Record.Author = author;
            }

            var siteName = context.Attribute(part.PartDefinition.Name, "SiteName");
            if (siteName != null) {
                part.Record.SiteName = siteName;
            }

            var userName = context.Attribute(part.PartDefinition.Name, "UserName");
            if (userName != null) {
                part.Record.UserName = userName;
            }

            var email = context.Attribute(part.PartDefinition.Name, "Email");
            if (email != null) {
                part.Record.Email = email;
            }

            var status = context.Attribute(part.PartDefinition.Name, "Status");
            if (status != null) {
                part.Record.Status = (CommentStatus) Enum.Parse(typeof(CommentStatus), status);
            }

            var commentDate = context.Attribute(part.PartDefinition.Name, "CommentDateUtc");
            if (commentDate != null) {
                part.Record.CommentDateUtc = XmlConvert.ToDateTime(commentDate, XmlDateTimeSerializationMode.Utc);
            }

            var text = context.Attribute(part.PartDefinition.Name, "CommentText");
            if (text != null) {
                part.Record.CommentText = text;
            }

            var commentedOn = context.Attribute(part.PartDefinition.Name, "CommentedOn");
            if (commentedOn != null) {
                var contentItem = context.GetItemFromSession(commentedOn);
                if (contentItem != null) {
                    part.Record.CommentedOn = contentItem.Id;
                }

                contentItem.As<CommentsPart>().Record.CommentPartRecords.Add(part.Record);
            }

            var commentedOnContainer = context.Attribute(part.PartDefinition.Name, "CommentedOnContainer");
            if (commentedOnContainer != null) {
                var container = context.GetItemFromSession(commentedOnContainer);
                if (container != null) {
                    part.Record.CommentedOnContainer = container.Id;
                }
            }
        }

        protected override void Exporting(CommentPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Author", part.Record.Author);
            context.Element(part.PartDefinition.Name).SetAttributeValue("SiteName", part.Record.SiteName);
            context.Element(part.PartDefinition.Name).SetAttributeValue("UserName", part.Record.UserName);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Email", part.Record.Email);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Status", part.Record.Status.ToString());

            if (part.Record.CommentDateUtc != null) {
                context.Element(part.PartDefinition.Name)
                    .SetAttributeValue("CommentDateUtc", XmlConvert.ToString(part.Record.CommentDateUtc.Value, XmlDateTimeSerializationMode.Utc));
            }
            context.Element(part.PartDefinition.Name).SetAttributeValue("CommentText", part.Record.CommentText);

            var commentedOn = _contentManager.Get(part.Record.CommentedOn);
            if (commentedOn != null) {
                var commentedOnIdentity = _contentManager.GetItemMetadata(commentedOn).Identity;
                context.Element(part.PartDefinition.Name).SetAttributeValue("CommentedOn", commentedOnIdentity.ToString());
            }

            var commentedOnContainer = _contentManager.Get(part.Record.CommentedOnContainer);
            if (commentedOnContainer != null) {
                var commentedOnContainerIdentity = _contentManager.GetItemMetadata(commentedOnContainer).Identity;
                context.Element(part.PartDefinition.Name).SetAttributeValue("CommentedOnContainer", commentedOnContainerIdentity.ToString());
            }
        }
    }
}
