using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Orchard.Blogs.Models;
using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Core.Navigation.Models;
using Orchard.Security;
using Orchard.Blogs.Services;
using Orchard.Core.Navigation.Services;
using Orchard.Settings;
using Orchard.Core.Title.Models;

namespace Orchard.Blogs.Commands {
    public class BlogCommands : DefaultOrchardCommandHandler {
        private readonly IContentManager _contentManager;
        private readonly IMembershipService _membershipService;
        private readonly IBlogService _blogService;
        private readonly IMenuService _menuService;
        private readonly ISiteService _siteService;

        public BlogCommands(
            IContentManager contentManager,
            IMembershipService membershipService,
            IBlogService blogService,
            IMenuService menuService,
            ISiteService siteService) {
            _contentManager = contentManager;
            _membershipService = membershipService;
            _blogService = blogService;
            _menuService = menuService;
            _siteService = siteService;
        }

        [OrchardSwitch]
        public string FeedUrl { get; set; }

        [OrchardSwitch]
        public int Id { get; set; }

        [OrchardSwitch]
        public string Owner { get; set; }

        [OrchardSwitch]
        public string Title { get; set; }

        [OrchardSwitch]
        public string Description { get; set; }

        [OrchardSwitch]
        public string MenuText { get; set; }

        [OrchardSwitch]
        public bool Homepage { get; set; }

        [CommandName("blog create")]
        [CommandHelp("blog create /Title:<title> [/Owner:<username>] [/Description:<description>] [/MenuText:<menu text>] [/Homepage:true|false]\r\n\t" + "Creates a new Blog")]
        [OrchardSwitches("Title,Owner,Description,MenuText,Homepage")]
        public void Create() {
            if (String.IsNullOrEmpty(Owner)) {
                Owner = _siteService.GetSiteSettings().SuperUser;
            }
            var owner = _membershipService.GetUser(Owner);

            if (owner == null) {
                Context.Output.WriteLine(T("Invalid username: {0}", Owner));
                return;
            }

            var blog = _contentManager.New("Blog");
            blog.As<ICommonPart>().Owner = owner;
            blog.As<TitlePart>().Title = Title;
            if (!String.IsNullOrEmpty(Description)) {
                blog.As<BlogPart>().Description = Description;
            }
            if ( !String.IsNullOrWhiteSpace(MenuText) ) {
                blog.As<MenuPart>().OnMainMenu = true;
                blog.As<MenuPart>().MenuPosition = _menuService.Get().Select(menuPart => menuPart.MenuPosition).Max() + 1 + ".0";
                blog.As<MenuPart>().MenuText = MenuText;
            }

            if (Homepage) {
                dynamic dblog = blog;
                if (dblog.AutoroutePart != null) {
                    dblog.AutoroutePart.UseCustomPattern = true;
                    dblog.AutoroutePart.CustomPattern = "";
                }
            }


            _contentManager.Create(blog);

            Context.Output.WriteLine(T("Blog created successfully"));
        }

        [CommandName("blog import")]
        [CommandHelp("blog import /BlogId:<id> /FeedUrl:<feed url> /Owner:<username>\r\n\t" + "Import all items from <feed url> into the blog specified by <id>")]
        [OrchardSwitches("FeedUrl,Id,Owner")]
        public void Import() {
            var owner = _membershipService.GetUser(Owner);

            if(owner == null) {
                Context.Output.WriteLine(T("Invalid username: {0}", Owner));
                return;
            }

            XDocument doc;

            try {
                Context.Output.WriteLine(T("Loading feed..."));
                doc = XDocument.Load(FeedUrl);
                Context.Output.WriteLine(T("Found {0} items", doc.Descendants("item").Count()));
            }
            catch (Exception ex) {
                throw new OrchardException(T("An error occured while loading the feed at {0}.", FeedUrl), ex);
            }

            var blog = _blogService.Get(Id,VersionOptions.Latest);

            if ( blog == null ) {
                Context.Output.WriteLine(T("Blog not found with specified Id: {0}", Id));
                return;
            }

            foreach ( var item in doc.Descendants("item") ) {
                if (item != null) {
                    var postName = item.Element("title").Value;

                    Context.Output.WriteLine(T("Adding post: {0}...", postName.Substring(0, Math.Min(postName.Length, 40))));
                    var post = _contentManager.New("BlogPost");
                    post.As<ICommonPart>().Owner = owner;
                    post.As<ICommonPart>().Container = blog;
                    post.As<TitlePart>().Title = postName;
                    post.As<BodyPart>().Text = item.Element("description").Value;
                    _contentManager.Create(post);
                }
            }

            Context.Output.WriteLine(T("Import feed completed."));
        }

    }
}