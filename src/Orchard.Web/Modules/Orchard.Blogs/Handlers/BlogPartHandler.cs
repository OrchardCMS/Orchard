using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Services;

namespace Orchard.Blogs.Handlers {
    [UsedImplicitly]
    public class BlogPartHandler : ContentHandler {
        private readonly IWorkContextAccessor _workContextAccessor;

        public BlogPartHandler(IRepository<BlogPartRecord> repository, IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
            Filters.Add(StorageFilter.For(repository));

            OnGetDisplayShape<BlogPart>((context, blog) => {
                context.Shape.Description = blog.Description;
                context.Shape.PostCount = blog.PostCount;
            });
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var blog = context.ContentItem.As<BlogPart>();

            if (blog == null)
                return;

            context.Metadata.DisplayRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Blogs"},
                {"Controller", "Blog"},
                {"Action", "Item"},
                {"blogId", context.ContentItem.Id}
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