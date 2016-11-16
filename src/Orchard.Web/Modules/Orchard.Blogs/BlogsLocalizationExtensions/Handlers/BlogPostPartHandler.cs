using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;

using Orchard.Autoroute.Models;
using Orchard.Autoroute.Services;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization.Models;

namespace Orchard.Blogs.BlogsLocalizationExtensions.Handlers {
    [OrchardFeature("Orchard.Blogs.LocalizationExtensions")]
    public class BlogPostPartHandler : ContentHandler {
        private readonly IContentManager _contentManager;
        private readonly IAutorouteService _routeService;

        public BlogPostPartHandler(RequestContext requestContext, IContentManager contentManager, IAutorouteService routeService) {
            _contentManager = contentManager;
            _routeService = routeService;

            //move posts when created, updated or published
            OnCreating<BlogPostPart>((context, part) => MigrateBlogPost(context.ContentItem));
            OnUpdating<BlogPostPart>((context, part) => MigrateBlogPost(context.ContentItem));
            OnPublishing<BlogPostPart>((context, part) => MigrateBlogPost(context.ContentItem));
        }

        //This Method checks the blog post's culture and it's parent blog's culture and moves it to the correct blog if they aren't equal.
        private void MigrateBlogPost(ContentItem item) {
            if (!item.Has<LocalizationPart>() || !item.Has<BlogPostPart>()) {
                return;
            }
            var blogItem = _contentManager.Get(item.As<CommonPart>().Container.Id);
            if (!blogItem.Has<LocalizationPart>() || blogItem.As<LocalizationPart>().Culture == null) {
                return;
            }

            //get our 2 cultures for comparison
            var blogCulture = blogItem.As<LocalizationPart>().Culture;
            var postCulture = item.As<LocalizationPart>().Culture;

            //if the post is a different culture than the parent blog change the post's parent blog to the right localization...
            if (postCulture != null && (postCulture.Id != blogCulture.Id)) {
                //Get the id of the current blog
                var blogids = new HashSet<int> { blogItem.As<BlogPart>().ContentItem.Id };

                //Add master blog id if current blog is a translation (child of the master blog)
                _contentManager.Query("Blog")
                               .Join<LocalizationPartRecord>()
                               .Where(x => x.MasterContentItemId == blogItem.As<BlogPart>().ContentItem.Id)
                               .List().ToList().ForEach(x => blogids.Add(x.Id));

                //and look in all master blog's translations
                if (blogItem.As<LocalizationPart>() != null && blogItem.As<LocalizationPart>().MasterContentItem != null) {
                    _contentManager.Query("Blog").Join<CommonPartRecord>().Where(x => x.Id == blogItem.As<LocalizationPart>().MasterContentItem.Id).List().ToList().ForEach(x => blogids.Add(x.Id));
                }

                foreach (var blogid in blogids) {
                    var thisBlog = _contentManager.Get(blogid);

                    //find this poor, orphaned, blogpost a new daddy
                    if (thisBlog.Has<LocalizationPart>() && thisBlog.As<LocalizationPart>().Culture.Id == postCulture.Id) {
                        item.As<ICommonPart>().Container = thisBlog;
                        if (item.Has<AutoroutePart>()) {
                            _routeService.RemoveAliases(item.As<AutoroutePart>());
                            item.As<AutoroutePart>().DisplayAlias = _routeService.GenerateAlias(item.As<AutoroutePart>());
                            _routeService.PublishAlias(item.As<AutoroutePart>());
                        }
                        return;
                    }
                }
            }
        }
    }
}