using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Events;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.Blogs.Models;
using Orchard.Core.Common.Models;

namespace Orchard.Blogs.Routing {
    public interface IRoutePatternProvider : IEventHandler {
        void Routed(IContent content, String path, ICollection<Tuple<string, RouteValueDictionary>> aliases);
    }
    public class BlogAutoroutes : IRoutePatternProvider {

        public void Routed(IContent content, string path, ICollection<Tuple<string, RouteValueDictionary>> aliases) {

            // TODO: (PH:Autoroute) Cheap and it works. But could there be a better way?
            if (content.Is<BlogPart>()) {
                // Add RSD route
                aliases.Add(Tuple.Create(
                    path + "/rsd",
                        new RouteValueDictionary {{"area", "Orchard.Blogs"},
                        {"controller", "RemoteBlogPublishing"},
                        {"action", "Rsd"},
                        {"blogId", content.Id}
                    }));
                aliases.Add(Tuple.Create(
                    path + "/Archive",
                        new RouteValueDictionary {
                        {"area", "Orchard.Blogs"},
                        {"controller", "BlogPost"},
                        {"action", "ListByArchive"},
                        {"blogId", content.Id},
                        {"archiveData", ""}
                    }));
            }

            if (content.Is<BlogPostPart>()) {
                var pathStart = "{Content.Container.Slug}/Archive/";

                // Add Archives routes
                var year = "{Content.Date.Format:yyyy}";
                var month = "{Content.Date.Format:MM}";
                var day = "{Content.Date.Format:dd}";

                aliases.Add(Tuple.Create(
                    pathStart + String.Format("{0}", year),
                        new RouteValueDictionary {
                        {"area", "Orchard.Blogs"},
                        {"controller", "BlogPost"},
                        {"action", "ListByArchive"},
                        {"blogId", content.As<CommonPart>().Container.Id},
                        {"archiveData", String.Format("{0}",year)}
                    }));
                aliases.Add(Tuple.Create(
                    pathStart + String.Format("{0}/{1}", year, month),
                        new RouteValueDictionary {
                        {"area", "Orchard.Blogs"},
                        {"controller", "BlogPost"},
                        {"action", "ListByArchive"},
                        {"blogId", content.As<CommonPart>().Container.Id},
                        {"archiveData", String.Format("{0}/{1}",year,month)}
                    }));
                aliases.Add(Tuple.Create(
                    pathStart + String.Format("{0}/{1}/{2}", year, month, day),
                        new RouteValueDictionary {
                        {"area", "Orchard.Blogs"},
                        {"controller", "BlogPost"},
                        {"action", "ListByArchive"},
                        {"blogId", content.As<CommonPart>().Container.Id},
                        {"archiveData", String.Format("{0}/{1}/{2}",year,month,day)}
                    }));
            }
        }
    }
}