using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Core.Navigation.Models;
using Orchard.Environment.Extensions;
using Orchard.Security;
using Orchard.Core.Title.Models;

namespace Orchard.Experimental.Commands {
    [OrchardFeature("Profiling")]
    public class ProfilingCommands : DefaultOrchardCommandHandler {
        private readonly IContentManager _contentManager;
        private readonly IMembershipService _membershipService;

        public ProfilingCommands(IContentManager contentManager, IMembershipService membershipService) {
            _contentManager = contentManager;
            _membershipService = membershipService;
        }

        [CommandName("add profiling data")]
        public void AddProfilingData() {
            var admin = _membershipService.GetUser("admin");

            for (var index = 0; index != 5; ++index) {
                var pageName = "page" + index;
                var page = _contentManager.Create("Page", VersionOptions.Draft);
                page.As<ICommonPart>().Owner = admin;
                page.As<TitlePart>().Title = pageName;
                page.As<BodyPart>().Text = pageName;
                page.As<MenuPart>().OnMainMenu = true;
                page.As<MenuPart>().MenuPosition = "5." + index;
                page.As<MenuPart>().MenuText = pageName;
                _contentManager.Publish(page);

                var blogName = "blog" + index;
                var blog = _contentManager.New("Blog");
                blog.As<ICommonPart>().Owner = admin;
                blog.As<TitlePart>().Title = blogName;
                blog.As<MenuPart>().OnMainMenu = true;
                blog.As<MenuPart>().MenuPosition = "6." + index;
                blog.As<MenuPart>().MenuText = blogName;
                _contentManager.Create(blog);

                // "BlogPost" content type can't be created w/out http context at the moment
                //for (var index2 = 0; index2 != 5; ++index2) {
                //    var postName = "post" + index;
                //    var post = _contentManager.New("BlogPost");
                //    post.As<ICommonPart>().Owner = admin;
                //    post.As<ICommonPart>().Container = blog;
                //    post.As<RoutableAspect>().Slug = postName;
                //    post.As<RoutableAspect>().Title = postName;
                //    post.As<BodyPart>().Text = postName;
                //    _contentManager.Create(post);
                //}
            }

            Context.Output.WriteLine(T("Finished adding profiling data"));
        }

        [CommandName("add users")]
        public void AddUsers() {
            for (int i = 0; i < 1000; i++) {
                var user = _membershipService.CreateUser(new CreateUserParams("user" + i, "1234567", "user" + i + "@orchardproject.net", null, null, true));
                if (user == null) {
                    Context.Output.WriteLine(T("Creating user failed. The authentication provider returned an error. Aborting..."));
                    return;
                }
            }
            Context.Output.WriteLine(T("Adding users completed successfully"));
        }
    }
}
