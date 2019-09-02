using System;
using System.Collections.Generic;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Orchard.Blogs.Drivers {
    public class BlogPartDriver : ContentPartDriver<BlogPart> {
        protected override string Prefix {
            get { return "BlogPart"; }
        }

        protected override DriverResult Display(BlogPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Blogs_Blog_Manage",
                    () => shapeHelper.Parts_Blogs_Blog_Manage()),
                ContentShape("Parts_Blogs_Blog_Description",
                    () => shapeHelper.Parts_Blogs_Blog_Description(Description: part.Description)),
                ContentShape("Parts_Blogs_Blog_SummaryAdmin",
                    () => shapeHelper.Parts_Blogs_Blog_SummaryAdmin()),
                ContentShape("Parts_Blogs_Blog_BlogPostCount",
                    () => shapeHelper.Parts_Blogs_Blog_BlogPostCount(PostCount: part.PostCount))
                );
        }

        protected override DriverResult Editor(BlogPart blogPart, dynamic shapeHelper) {
            var results = new List<DriverResult> {
                ContentShape("Parts_Blogs_Blog_Fields",
                             () => shapeHelper.EditorTemplate(TemplateName: "Parts.Blogs.Blog.Fields", Model: blogPart, Prefix: Prefix))
            };

            
            if (blogPart.Id > 0)
                results.Add(ContentShape("Blog_DeleteButton",
                    deleteButton => deleteButton));

            return Combined(results.ToArray());
        }

        protected override DriverResult Editor(BlogPart blogPart, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(blogPart, Prefix, null, null);
            return Editor(blogPart, shapeHelper);
        }

        protected override void Importing(BlogPart part, ContentManagement.Handlers.ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "Description", description =>
                part.Description = description
            );

            context.ImportAttribute(part.PartDefinition.Name, "PostCount", postCount =>
                part.PostCount = Convert.ToInt32(postCount)
            );

            context.ImportAttribute(part.PartDefinition.Name, "FeedProxyUrl", feedProxyUrl =>
                part.FeedProxyUrl = feedProxyUrl
            );
        }

        protected override void Exporting(BlogPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Description", part.Description);
            context.Element(part.PartDefinition.Name).SetAttributeValue("PostCount", part.PostCount);
            context.Element(part.PartDefinition.Name).SetAttributeValue("FeedProxyUrl", part.FeedProxyUrl);
        }
    }
}