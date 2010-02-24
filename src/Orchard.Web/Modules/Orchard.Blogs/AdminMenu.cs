using System.Linq;
using Orchard.Blogs.Services;
using Orchard.UI.Navigation;

namespace Orchard.Blogs {
    public class AdminMenu : INavigationProvider {
        private readonly IBlogService _blogService;

        public AdminMenu(IBlogService blogService) {
            _blogService = blogService;
        }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add("Blogs", "2", BuildMenu);
        }

        private void BuildMenu(NavigationItemBuilder menu) {
            var blogs = _blogService.Get();
            var singleBlog = blogs.Count() == 1 ? blogs.ElementAt(0) : null;

            if (singleBlog == null)
                menu.Add("Manage Blogs", "1.0",
                         item =>
                         item.Action("List", "BlogAdmin", new {area = "Orchard.Blogs"}).Permission(Permissions.MetaListBlogs));
            else
                menu.Add("Manage Blog", "1.0",
                    item =>
                    item.Action("Item", "BlogAdmin", new {area = "Orchard.Blogs", blogSlug = singleBlog.Slug}).Permission(Permissions.MetaListBlogs));

            menu.Add("Add New Blog", "1.1",
                     item =>
                     item.Action("Create", "BlogAdmin", new {area = "Orchard.Blogs"}).Permission(Permissions.ManageBlogs));

            if (singleBlog != null)
                menu.Add("Add New Post", "1.2",
                         item =>
                         item.Action("Create", "BlogPostAdmin", new {area = "Orchard.Blogs", blogSlug = singleBlog.Slug}).Permission(Permissions.PublishBlogPost));
        }
    }
}