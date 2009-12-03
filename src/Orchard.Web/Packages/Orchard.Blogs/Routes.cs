using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Blogs.Routing;
using Orchard.Blogs.Services;
using Orchard.Mvc.Routes;

namespace Orchard.Blogs {
    public class Routes : IRouteProvider
    {
        private readonly IBlogService _blogService;

        public Routes(IBlogService blogService) {
            _blogService = blogService;
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/Blogs/Create",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"},
                                                                                      {"controller", "Blog"},
                                                                                      {"action", "Create"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/Blogs/{blogSlug}/Edit",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"},
                                                                                      {"controller", "Blog"},
                                                                                      {"action", "Edit"}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"blogSlug", new IsBlogConstraint(_blogService)}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/Blogs/{blogSlug}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"},
                                                                                      {"controller", "Blog"},
                                                                                      {"action", "ItemForAdmin"}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"blogSlug", new IsBlogConstraint(_blogService)}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/Blogs/{blogSlug}/Posts/Create",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"},
                                                                                      {"controller", "BlogPost"},
                                                                                      {"action", "Create"}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"blogSlug", new IsBlogConstraint(_blogService)}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/Blogs/{blogSlug}/Posts/{postSlug}/Edit",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"},
                                                                                      {"controller", "BlogPost"},
                                                                                      {"action", "Edit"}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"blogSlug", new IsBlogConstraint(_blogService)}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/Blogs/{blogSlug}/Posts",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"},
                                                                                      {"controller", "BlogPost"},
                                                                                      {"action", "ListForAdmin"}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"blogSlug", new IsBlogConstraint(_blogService)}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/Blogs",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"},
                                                                                      {"controller", "Blog"},
                                                                                      {"action", "ListForAdmin"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Blogs",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"},
                                                                                      {"controller", "Blog"},
                                                                                      {"action", "List"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "{blogSlug}/{postSlug}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"},
                                                                                      {"controller", "BlogPost"},
                                                                                      {"action", "Item"}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"blogSlug", new IsBlogConstraint(_blogService)}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "{blogSlug}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"},
                                                                                      {"controller", "Blog"},
                                                                                      {"action", "Item"}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"blogSlug", new IsBlogConstraint(_blogService)}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 }
                         };
        }
    }
}