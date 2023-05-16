using Orchard.Blogs.Services;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Orchard.Blogs {
    public class BlogPostsLocalNavigationProvider : INavigationProvider {
        private readonly IBlogService _blogService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IWorkContextAccessor _workContextAccessor;

        public BlogPostsLocalNavigationProvider(
            IBlogService blogService,
            IAuthorizationService authorizationService,
            IWorkContextAccessor workContextAccessor) {

            T = NullLocalizer.Instance;
            _blogService = blogService;
            _authorizationService = authorizationService;
            _workContextAccessor = workContextAccessor;
        }

        public Localizer T { get; set; }

        public string MenuName {
            get { return "blogposts-navigation"; }
        }

        public void GetNavigation(NavigationBuilder builder) {
            var blogId = 0;
            int.TryParse(_workContextAccessor.GetContext().HttpContext.Request.RequestContext.RouteData.Values["blogId"]?.ToString(), out blogId);
            if (blogId > 0) {
                builder.Add(T("Blog posts"),
                    item => item.Action("Item", "BlogAdmin", new { area = "Orchard.Blogs", blogId })
                        .LocalNav()
                        .Permission(Permissions.MetaListOwnBlogs));
            }
        }
    }
}