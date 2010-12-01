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
        private readonly IBlogSlugConstraint _blogSlugConstraint;
        private readonly IHomePageProvider _routableHomePageProvider;

        public BlogPartHandler(IRepository<BlogPartRecord> repository, IWorkContextAccessor workContextAccessor, IEnumerable<IHomePageProvider> homePageProviders, IBlogSlugConstraint blogSlugConstraint) {
            _workContextAccessor = workContextAccessor;
            _blogSlugConstraint = blogSlugConstraint;
            _routableHomePageProvider = homePageProviders.SingleOrDefault(p => p.GetProviderName() == RoutableHomePageProvider.Name);
            Filters.Add(StorageFilter.For(repository));

            OnPublished<RoutePart>((context, route) => {
                if (route.Is<BlogPart>()) {
                    if (route.ContentItem.Id != 0 && route.PromoteToHomePage)
                        _blogSlugConstraint.AddSlug("");
                }
                else if (route.ContentItem.Id != 0 && route.PromoteToHomePage) {
                    _blogSlugConstraint.RemoveSlug("");
                }
            });

            OnGetDisplayShape<BlogPart>((context, blog) => {
                                            context.Shape.Description = blog.Description;
                                            context.Shape.PostCount = blog.PostCount;
                                        });
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var blog = context.ContentItem.As<BlogPart>();

            if (blog == null)
                return;

            var blogSlug = blog.Id == _routableHomePageProvider.GetHomePageId(_workContextAccessor.GetContext().CurrentSite.HomePage)
                ? ""
                : blog.As<RoutePart>().Slug;

            context.Metadata.DisplayRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Blogs"},
                {"Controller", "Blog"},
                {"Action", "Item"},
                {"blogSlug", blogSlug}
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
                {"Id", context.ContentItem.Id}
            };
            context.Metadata.RemoveRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Blogs"},
                {"Controller", "BlogAdmin"},
                {"Action", "Remove"},
                {"Id", context.ContentItem.Id}
            };
        }
    }
}