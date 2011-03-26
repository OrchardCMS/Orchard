using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.Blogs.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Routable.Models;
using Orchard.Core.Routable.Services;
using Orchard.Data;
using Orchard.Services;

namespace Orchard.Blogs.Handlers {
    [UsedImplicitly]
    public class BlogPartHandler : ContentHandler {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IBlogPathConstraint _blogPathConstraint;
        private readonly IHomePageProvider _routableHomePageProvider;

        public BlogPartHandler(IRepository<BlogPartRecord> repository, IWorkContextAccessor workContextAccessor, IEnumerable<IHomePageProvider> homePageProviders, IBlogPathConstraint blogPathConstraint) {
            _workContextAccessor = workContextAccessor;
            _blogPathConstraint = blogPathConstraint;
            _routableHomePageProvider = homePageProviders.SingleOrDefault(p => p.GetProviderName() == RoutableHomePageProvider.Name);
            Filters.Add(StorageFilter.For(repository));

            Action<PublishContentContext, RoutePart> publishedHandler = (context, route) => {
                if (route.Is<BlogPart>()) {
                    if (route.ContentItem.Id != 0 && route.PromoteToHomePage)
                        _blogPathConstraint.AddPath("");
                }
                else if (route.ContentItem.Id != 0 && route.PromoteToHomePage) {
                    _blogPathConstraint.RemovePath("");
                }
            };

            OnPublished<RoutePart>(publishedHandler);
            OnUnpublished<RoutePart>(publishedHandler);

            OnGetDisplayShape<BlogPart>((context, blog) => {
                context.Shape.Description = blog.Description;
                context.Shape.PostCount = blog.PostCount;
            });
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var blog = context.ContentItem.As<BlogPart>();

            if (blog == null)
                return;

            var blogPath = blog.Id == _routableHomePageProvider.GetHomePageId(_workContextAccessor.GetContext().CurrentSite.HomePage)
                ? ""
                : blog.As<RoutePart>().Path;

            context.Metadata.DisplayRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Blogs"},
                {"Controller", "Blog"},
                {"Action", "Item"},
                {"blogPath", blogPath}
            };
            context.Metadata.CreateRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Blogs"},
                {"Controller", "BlogAdmin"},
                {"Action", "Create"}
            };
            context.Metadata.EditorRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Blogs"},
                {"Controller", "BlogAdmin"},
                {"Action", "Edit"},
                {"blogId", context.ContentItem.Id}
            };
            context.Metadata.RemoveRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Blogs"},
                {"Controller", "BlogAdmin"},
                {"Action", "Remove"},
                {"blogId", context.ContentItem.Id}
            };
            context.Metadata.AdminRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Blogs"},
                {"Controller", "BlogAdmin"},
                {"Action", "Item"},
                {"blogId", context.ContentItem.Id}
            };
        }
    }
}