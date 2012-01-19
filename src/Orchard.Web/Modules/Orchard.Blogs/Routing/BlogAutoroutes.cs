using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Events;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.Blogs.Models;
using Orchard.Core.Common.Models;
using Orchard.Autoroute.Services;
using Orchard.Localization;

namespace Orchard.Blogs.Routing {
    public class BlogAutoroutes : IRoutePatternProvider {

        public BlogAutoroutes() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        const string yearToken = "{Content.Date.Format:yyyy}";
        const string monthToken = "{Content.Date.Format:MM}";
        const string dayToken = "{Content.Date.Format:dd}";
        const string idParentToken = "{Content.Container.Id}";
        const string idToken = "{Content.Id}";
        const string blogToken = "{Content.Slug}";
        const string blogPostToken = "{Content.Container.Path}/{Content.Slug}";

        public void Describe(DescribePatternContext describe) {
            // TODO: (PH) Could implement RSD for non-blog content much more easily now the routing can be applied to any content item... (maybe need a RemotePublishingPart?)
            // TODO: Must restrict these to appropriate parts/types...
            describe.For("Content")
                //                .Where<IContent>(c => c.Is<BlogPart>())
                .Pattern("Rsd", T("Remote Blog Publishing"), T("Remote Blog Publishing destination Url"),
                    new RouteValueDictionary {
                            {"area", "Orchard.Blogs"},
                            {"controller", "RemoteBlogPublishing"},
                            {"action", "Rsd"},
                            {"blogId", idToken}})
                .Pattern("Archive", T("Blog Archives"), T("Displays a list of all blog archives"),
                    new RouteValueDictionary {
                        {"area", "Orchard.Blogs"},
                        {"controller", "BlogPost"},
                        {"action", "ListByArchive"},
                        {"blogId", idToken},
                        {"archiveData", ""}
                    })
                    ;
            describe.For("Content")
   //             .Where<IContent>(c => c.Is<BlogPostPart>())
                .Pattern("ArchiveYear", T("Blog Archives by Year"), T("Displays a list of all blog archives for a particular year"),
                        new RouteValueDictionary {
                        {"area", "Orchard.Blogs"},
                        {"controller", "BlogPost"},
                        {"action", "ListByArchive"},
                        {"blogId", idParentToken},
                        {"archiveData", String.Format("{0}",yearToken)}
                    })
                .Pattern("ArchiveMonth", T("Blog Archives by Month"), T("Displays a list of all blog archives for a particular year and month"),
                        new RouteValueDictionary {
                        {"area", "Orchard.Blogs"},
                        {"controller", "BlogPost"},
                        {"action", "ListByArchive"},
                        {"blogId", idParentToken},
                        {"archiveData", String.Format("{0}/{1}",yearToken,monthToken)}
                    })
                .Pattern("ArchiveDay", T("Blog Archives by Day"), T("Displays a list of all blog archives for a particular date"),
                        new RouteValueDictionary {
                        {"area", "Orchard.Blogs"},
                        {"controller", "BlogPost"},
                        {"action", "ListByArchive"},
                        {"blogId", idParentToken},
                        {"archiveData", String.Format("{0}/{1}/{2}",yearToken,monthToken,dayToken)}
                    });
        }

        public void Suggest(SuggestPatternContext suggest) {
            suggest.For("Content")
                .Suggest("Rsd", "blog-title/rsd", blogToken + "/rsd", T("Rsd is a sub-path of the blog post"))
                .Suggest("Archives", "blog-title/archives", blogToken + "/archives", T("Archives is a sub-path of the blog post"))
                .Suggest("View", "blog-title", blogToken, T("Blog title"))
                .Suggest("View", "blog-title/post-title", blogPostToken, T("Nested blog/post path"))
                .Suggest("ArchivesYear", "blog-title/post-title/archives/yy", String.Format("{0}/archives/{1}",blogPostToken,yearToken), T("Archives year"))
                .Suggest("ArchivesMonth", "blog-title/post-title/archives/yy/mm", String.Format("{0}/archives/{1}/{2}",blogPostToken,yearToken,monthToken), T("Archives year/month"))
                .Suggest("ArchivesDay", "blog-title/post-title/archives/yy/mm/dd", String.Format("{0}/archives/{1}/{2}/{3}", blogPostToken, yearToken,monthToken,dayToken), T("Archives year/month/day"));
        }

    }
}