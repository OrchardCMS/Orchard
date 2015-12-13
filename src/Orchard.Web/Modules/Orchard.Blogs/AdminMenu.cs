using System.Linq;
using Orchard.Blogs.Services;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Orchard.Blogs {
    public class AdminMenu : INavigationProvider {
        private readonly IBlogService _blogService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IWorkContextAccessor _workContextAccessor;

        public AdminMenu(IBlogService blogService, IAuthorizationService authorizationService, IWorkContextAccessor workContextAccessor) {
            _blogService = blogService;
            _authorizationService = authorizationService;
            _workContextAccessor = workContextAccessor;
        }

        public Localizer T { get; set; }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.AddImageSet("blog")
                .Add(T("Blog"), "1.5", BuildMenu);
        }

        private void BuildMenu(NavigationItemBuilder menu) {
            var blogs = _blogService.Get().Where(x => _authorizationService.TryCheckAccess(Permissions.MetaListBlogs, _workContextAccessor.GetContext().CurrentUser, x)).ToArray();
            var blogCount = blogs.Count();
            var singleBlog = blogCount == 1 ? blogs.ElementAt(0) : null;

            if (blogCount > 0 && singleBlog == null) {
                menu.Add(T("Manage Blogs"), "3",
                         item => item.Action("List", "BlogAdmin", new { area = "Orchard.Blogs" }).Permission(Permissions.MetaListOwnBlogs));
            }
            else if (singleBlog != null)
                menu.Add(T("Manage Blog"), "1.0",
                    item => item.Action("Item", "BlogAdmin", new { area = "Orchard.Blogs", blogId = singleBlog.Id }).Permission(Permissions.MetaListOwnBlogs));

            if (singleBlog != null)
                menu.Add(T("New Post"), "1.1",
                         item =>
                         item.Action("Create", "BlogPostAdmin", new {area = "Orchard.Blogs", blogId = singleBlog.Id}).Permission(Permissions.MetaListOwnBlogs));

            menu.Add(T("New Blog"), "1.2",
                     item =>
                     item.Action("Create", "BlogAdmin", new { area = "Orchard.Blogs" }).Permission(Permissions.ManageBlogs));

        }
    }
}