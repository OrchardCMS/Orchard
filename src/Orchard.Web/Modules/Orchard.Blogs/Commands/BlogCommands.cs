using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Routable.Models;
using Orchard.Security;
using Orchard.Blogs.Services;
using Orchard.Core.Navigation.Services;

namespace Orchard.Blogs.Commands {
    public class BlogCommands : DefaultOrchardCommandHandler {
        private readonly IContentManager _contentManager;
        private readonly IMembershipService _membershipService;
        private readonly IBlogService _blogService;
        private readonly IMenuService _menuService;

        public BlogCommands(
            IContentManager contentManager,
            IMembershipService membershipService,
            IBlogService blogService,
            IMenuService menuService) {
            _contentManager = contentManager;
            _membershipService = membershipService;
            _blogService = blogService;
            _menuService = menuService;
        }

        [OrchardSwitch]
        public string FeedUrl { get; set; }

        [OrchardSwitch]
        public string Slug { get; set; }

        [OrchardSwitch]
        public string Title { get; set; }

        [OrchardSwitch]
        public string MenuText { get; set; }

        [CommandName("blog create")]
        [CommandHelp("blog create /Slug:<slug> /Title:<title> [/MenuText:<menu text>]\r\n\t" + "Creates a new Blog")]
        [OrchardSwitches("Slug,Title,MenuText")]
        public string Create() {
            var admin = _membershipService.GetUser("admin");

            if(!IsSlugValid(Slug)) {
                return "Invalid Slug provided. Blog creation failed.";    
            }

            var blog = _contentManager.New("Blog");
            blog.As<ICommonAspect>().Owner = admin;
            blog.As<IsRoutable>().Slug = Slug;
            blog.As<IsRoutable>().Title = Title;
            if ( !String.IsNullOrWhiteSpace(MenuText) ) {
                blog.As<MenuPart>().OnMainMenu = true;
                blog.As<MenuPart>().MenuPosition = _menuService.Get().Select(menuPart => menuPart.MenuPosition).Max() + 1 + ".0";
                blog.As<MenuPart>().MenuText = MenuText;
            }
            _contentManager.Create(blog);

            return "Blog created successfully";
        }

        [CommandName("blog import")]
        [CommandHelp("blog import /Slug:<slug> /FeedUrl:<feed url>\r\n\t" + "Import all items from <feed url> into the blog at the specified <slug>")]
        [OrchardSwitches("FeedUrl,Slug")]
        public string Import() {
            var admin = _membershipService.GetUser("admin");

            XDocument doc;

            try {
                Context.Output.WriteLine("Loading feed...");
                doc = XDocument.Load(FeedUrl);
                Context.Output.WriteLine("Found {0} items", doc.Descendants("item").Count());
            }
            catch ( Exception ex ) {
                Context.Output.WriteLine(T("An error occured while loading the file: " + ex.Message));
                return "Import terminated.";
            }

            var blog = _blogService.Get(Slug);

            if ( blog == null ) {
                return "Blog not found at specified slug: " + Slug;
            }

            foreach ( var item in doc.Descendants("item") ) {
                string postName = item.Element("title").Value;

                Context.Output.WriteLine("Adding post: {0}...", postName.Substring(0, Math.Min(postName.Length, 40)));
                var post = _contentManager.New("BlogPost");
                post.As<ICommonAspect>().Owner = admin;
                post.As<ICommonAspect>().Container = blog;
                post.As<IsRoutable>().Slug = Slugify(postName);
                post.As<IsRoutable>().Title = postName;
                post.As<BodyAspect>().Text = item.Element("description").Value;
                _contentManager.Create(post);
            }


            return "Import feed completed.";
        }

        private static string Slugify(string slug) {
            var dissallowed = new Regex(@"[/:?#\[\]@!$&'()*+,;=\s]+");

            slug = dissallowed.Replace(slug, "-");
            slug = slug.Trim('-');

            if ( slug.Length > 1000 )
                slug = slug.Substring(0, 1000);

            return slug.ToLowerInvariant();
        }

        private static bool IsSlugValid(string slug) {
            // see http://tools.ietf.org/html/rfc3987 for prohibited chars
            return slug == null || String.IsNullOrEmpty(slug.Trim()) || Regex.IsMatch(slug, @"^[^/:?#\[\]@!$&'()*+,;=\s]+$");
        }
    }
}