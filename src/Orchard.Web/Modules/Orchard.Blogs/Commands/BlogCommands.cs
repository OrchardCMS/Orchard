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
using Orchard.Core.Routable.Services;
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
        public string Owner { get; set; }

        [OrchardSwitch]
        public string Title { get; set; }

        [OrchardSwitch]
        public string MenuText { get; set; }

        [CommandName("blog create")]
        [CommandHelp("blog create /Slug:<slug> /Title:<title> /Owner:<username> [/MenuText:<menu text>]\r\n\t" + "Creates a new Blog")]
        [OrchardSwitches("Slug,Title,Owner,MenuText")]
        public string Create() {
            var owner = _membershipService.GetUser(Owner);

            if ( owner == null ) {
                Context.Output.WriteLine();
                return T("Invalid username: {0}", Owner).Text;
            }

            if(!IsSlugValid(Slug)) {
                return "Invalid Slug provided. Blog creation failed.";    
            }

            var blog = _contentManager.New("Blog");
            blog.As<ICommonPart>().Owner = owner;
            blog.As<RoutePart>().Slug = Slug;
            blog.As<RoutePart>().Path = Slug;
            blog.As<RoutePart>().Title = Title;
            if ( !String.IsNullOrWhiteSpace(MenuText) ) {
                blog.As<MenuPart>().OnMainMenu = true;
                blog.As<MenuPart>().MenuPosition = _menuService.Get().Select(menuPart => menuPart.MenuPosition).Max() + 1 + ".0";
                blog.As<MenuPart>().MenuText = MenuText;
            }
            _contentManager.Create(blog);

            return "Blog created successfully";
        }

        [CommandName("blog import")]
        [CommandHelp("blog import /Slug:<slug> /FeedUrl:<feed url> /Owner:<username>\r\n\t" + "Import all items from <feed url> into the blog at the specified <slug>")]
        [OrchardSwitches("FeedUrl,Slug,Owner")]
        public string Import() {
            var owner = _membershipService.GetUser(Owner);

            if(owner == null) {
                Context.Output.WriteLine();
                return T("Invalid username: {0}", Owner).Text;
            }

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
                if (item != null) {
                    string postName = item.Element("title").Value;

                    Context.Output.WriteLine("Adding post: {0}...", postName.Substring(0, Math.Min(postName.Length, 40)));
                    var post = _contentManager.New("BlogPost");
                    post.As<ICommonPart>().Owner = owner;
                    post.As<ICommonPart>().Container = blog;
                    var slug = Slugify(postName);
                    post.As<RoutePart>().Slug = slug;
                    post.As<RoutePart>().Path = post.As<RoutePart>().GetPathWithSlug(slug);
                    post.As<RoutePart>().Title = postName;
                    post.As<BodyPart>().Text = item.Element("description").Value;
                    _contentManager.Create(post);
                }
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