using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Blogs.Routing;
using Orchard.Mvc.Routes;

namespace Orchard.Blogs {
    public class Routes : IRouteProvider {
        private readonly IArchiveConstraint _archiveConstraint;
        private readonly IRsdConstraint _rsdConstraint;

        public Routes(
            IArchiveConstraint archiveConstraint,
            IRsdConstraint rsdConstraint) {
            _archiveConstraint = archiveConstraint;
            _rsdConstraint = rsdConstraint;
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            var routeDescriptors = new[] {
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/Blogs/Create",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"},
                                                                                      {"controller", "BlogAdmin"},
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
                                                         "Admin/Blogs/{blogId}/Edit",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"},
                                                                                      {"controller", "BlogAdmin"},
                                                                                      {"action", "Edit"}
                                                                                  },
                                                         new RouteValueDictionary (),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/Blogs/{blogId}/Remove",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"},
                                                                                      {"controller", "BlogAdmin"},
                                                                                      {"action", "Remove"}
                                                                                  },
                                                         new RouteValueDictionary (),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/Blogs/{blogId}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"},
                                                                                      {"controller", "BlogAdmin"},
                                                                                      {"action", "Item"}
                                                                                  },
                                                         new RouteValueDictionary (),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/Blogs/{blogId}/Posts/Create",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"},
                                                                                      {"controller", "BlogPostAdmin"},
                                                                                      {"action", "Create"}
                                                                                  },
                                                         new RouteValueDictionary (),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/Blogs/{blogId}/Posts/{postId}/Edit",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"},
                                                                                      {"controller", "BlogPostAdmin"},
                                                                                      {"action", "Edit"}
                                                                                  },
                                                         new RouteValueDictionary (),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/Blogs/{blogId}/Posts/{postId}/Delete",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"},
                                                                                      {"controller", "BlogPostAdmin"},
                                                                                      {"action", "Delete"}
                                                                                  },
                                                         new RouteValueDictionary (),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/Blogs/{blogId}/Posts/{postId}/Publish",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"},
                                                                                      {"controller", "BlogPostAdmin"},
                                                                                      {"action", "Publish"}
                                                                                  },
                                                         new RouteValueDictionary (),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/Blogs/{blogId}/Posts/{postId}/Unpublish",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"},
                                                                                      {"controller", "BlogPostAdmin"},
                                                                                      {"action", "Unpublish"}
                                                                                  },
                                                         new RouteValueDictionary (),
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
                                                                                      {"controller", "BlogAdmin"},
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
                                                         "{*path}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"},
                                                                                      {"controller", "BlogPost"},
                                                                                      {"action", "ListByArchive"}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"path", _archiveConstraint},
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Priority = 11,
                                                     Route = new Route(
                                                         "{*path}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"},
                                                                                      {"controller", "RemoteBlogPublishing"},
                                                                                      {"action", "Rsd"}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"path", _rsdConstraint}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"area", "Orchard.Blogs"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 }
                         };

            foreach (var routeDescriptor in routeDescriptors)
                routes.Add(routeDescriptor);
        }
    }
}