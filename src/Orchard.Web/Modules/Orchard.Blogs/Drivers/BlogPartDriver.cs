using System.Linq;
using JetBrains.Annotations;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Feeds;
using Orchard.Localization;

namespace Orchard.Blogs.Drivers {
    [UsedImplicitly]
    public class BlogPartDriver : ContentPartDriver<BlogPart> {
        public IOrchardServices Services { get; set; }

        private readonly IContentManager _contentManager;
        private readonly IBlogPostService _blogPostService;
        private readonly IFeedManager _feedManager;

        public BlogPartDriver(
            IOrchardServices services,
            IContentManager contentManager, 
            IBlogPostService blogPostService, 
            IFeedManager feedManager) {
            Services = services;
            _contentManager = contentManager;
            _blogPostService = blogPostService;
            _feedManager = feedManager;
            T = NullLocalizer.Instance;
        }


        public Localizer T { get; set; }

        protected override string Prefix { get { return ""; } }

        protected override DriverResult Display(BlogPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Blogs_Blog_Manage",
                             () => shapeHelper.Parts_Blogs_Blog_Manage(ContentPart: part)),
                ContentShape("Parts_Blogs_Blog_Description",
                             () => shapeHelper.Parts_Blogs_Blog_Description(ContentPart: part, Description: part.Description)),
                ContentShape("Parts_Blogs_Blog_BlogPostCount",
                             () => shapeHelper.Parts_Blogs_Blog_BlogPostCount(ContentPart: part, PostCount: part.PostCount))
                //,
                // todo: (heskew) implement a paging solution that doesn't require blog posts to be tied to the blog within the controller
                //ContentShape("Parts_Blogs_BlogPost_List",
                //             () => {
                //                 _feedManager.Register(part);
                //                 var list = shapeHelper.List();
                //                 list.AddRange(_blogPostService.Get(part)
                //                                           .Select(bp => _contentManager.BuildDisplay(bp, "Summary")));
                //                 return shapeHelper.Parts_Blogs_BlogPost_List(ContentPart: part, ContentItems: list);
                //             }),
                //ContentShape("Parts_Blogs_BlogPost_List_Admin",
                //             () =>
                //             {
                //                 var list = shapeHelper.List();
                //                 list.AddRange(_blogPostService.Get(part, VersionOptions.Latest)
                //                                           .Select(bp => _contentManager.BuildDisplay(bp, "SummaryAdmin")));
                //                 return shapeHelper.Parts_Blogs_BlogPost_List_Admin(ContentPart: part, ContentItems: list);
                //             })
                );
        }

        protected override DriverResult Editor(BlogPart blogPart, dynamic shapeHelper) {
            return ContentShape("Parts_Blogs_Blog_Fields",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts.Blogs.Blog.Fields", Model: blogPart, Prefix: Prefix));
        }

        protected override DriverResult Editor(BlogPart blogPart, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(blogPart, Prefix, null, null);
            return Editor(blogPart, shapeHelper);
        }
    }
}