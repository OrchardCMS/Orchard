using System;
using System.Linq;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;

namespace Orchard.Blogs.Commands {
    public class BlogWidgetCommands : DefaultOrchardCommandHandler {
        private readonly IWidgetsService _widgetsService;
        private readonly IBlogService _blogService;
        private readonly ISiteService _siteService;
        private readonly IMembershipService _membershipService;
        private readonly IContentManager _contentManager;

        private BlogPart blog;

        public BlogWidgetCommands(
            IWidgetsService widgetsService, 
            IBlogService blogService,
            ISiteService siteService, 
            IMembershipService membershipService,
            IContentManager contentManager) {
            _widgetsService = widgetsService;
            _blogService = blogService;
            _siteService = siteService;
            _membershipService = membershipService;
            _contentManager = contentManager;

            RenderTitle = true;
        }

        [OrchardSwitch]
        public string Title { get; set; }

        [OrchardSwitch]
        public string Name { get; set; }

        [OrchardSwitch]
        public bool RenderTitle { get; set; }

        [OrchardSwitch]
        public string Zone { get; set; }

        [OrchardSwitch]
        public string Position { get; set; }

        [OrchardSwitch]
        public string Layer { get; set; }

        [OrchardSwitch]
        public string Identity { get; set; }

        [OrchardSwitch]
        public string Owner { get; set; }

        [OrchardSwitch]
        public string BlogPath { get; set; }

        [OrchardSwitch]
        public int BlogId { get; set; }

        [OrchardSwitch]
        public string Count { get; set; }

        [CommandName("blog widget create recentblogposts")]
        [CommandHelp("blog widget create recentblogposts /Title:<title> /Name:<name> /Zone:<zone> /Position:<position> /Layer:<layer> (/BlogId:<id> | /BlogPath:<path>) [/Identity:<identity>] [/RenderTitle:true|false] [/Owner:<owner>] [/Count:<count>]\r\n\t" + "Creates a new widget")]
        [OrchardSwitches("Title,Name,Zone,Position,Layer,BlogId,BlogPath,Identity,Owner,RenderTitle,Count")]
        public void CreateRecentBlogPostsWidget() {
            var type = "RecentBlogPosts";

            var widget = CreateStandardWidget(type);
            if (widget == null) {
                return;
            }

            widget.As<RecentBlogPostsPart>().BlogId = blog.Id;

            // Setting count to 0 means all posts. It's an optional parameter and defaults to 5.
            if (!string.IsNullOrWhiteSpace(Count)) {
                int CountAsNumber = 0;
                if (Int32.TryParse(Count, out CountAsNumber)) {
                    widget.As<RecentBlogPostsPart>().Count = CountAsNumber;
                }
            }

            _contentManager.Publish(widget.ContentItem);
            Context.Output.WriteLine(T("{0} widget created successfully.", type).Text);
        }

        [CommandName("blog widget create blogarchives")]
        [CommandHelp("blog widget create blogarchives /Title:<title> /Name:<name> /Zone:<zone> /Position:<position> /Layer:<layer> (/BlogId:<id> | /BlogPath:<path>) [/Identity:<identity>] [/RenderTitle:true|false] [/Owner:<owner>]\r\n\t" + "Creates a new widget")]
        [OrchardSwitches("Title,Name,Zone,Position,Layer,BlogId,BlogPath,Identity,Owner,RenderTitle")]
        public void CreateBlogArchivesWidget() {
            var type = "BlogArchives";

            var widget = CreateStandardWidget(type);
            if (widget == null) {
                return;
            }

            widget.As<BlogArchivesPart>().BlogId = blog.Id;

            _contentManager.Publish(widget.ContentItem);
            Context.Output.WriteLine(T("{0} widget created successfully.", type).Text);
        }

        private WidgetPart CreateStandardWidget(string type) {
            var widgetTypeNames = _widgetsService.GetWidgetTypeNames().ToList();
            if (!widgetTypeNames.Contains(type)) {
                Context.Output.WriteLine(T("Creating widget failed: type {0} was not found. Supported widget types are: {1}.",
                    type,
                    string.Join(" ", widgetTypeNames)));
                return null;
            }

            var layer = GetLayer(Layer);
            if (layer == null) {
                Context.Output.WriteLine(T("Creating {0} widget failed: layer {1} was not found.", type, Layer));
                return null;
            }

            blog = GetBlog(BlogId, BlogPath);
            if (blog == null) {
                Context.Output.WriteLine(T("Creating {0} widget failed: blog was not found.", type));
                return null;
            }

            var widget = _widgetsService.CreateWidget(layer.ContentItem.Id, type, T(Title).Text, Position, Zone);

            if (!String.IsNullOrWhiteSpace(Name)) {
                widget.Name = Name.Trim();
            }

            widget.RenderTitle = RenderTitle;

            if (String.IsNullOrEmpty(Owner)) {
                Owner = _siteService.GetSiteSettings().SuperUser;
            }
            var owner = _membershipService.GetUser(Owner);
            widget.As<ICommonPart>().Owner = owner;

            if (widget.Has<IdentityPart>() && !String.IsNullOrEmpty(Identity)) {
                widget.As<IdentityPart>().Identifier = Identity;
            }

            return widget;
        }

        private LayerPart GetLayer(string layer) {
            var layers = _widgetsService.GetLayers();
            return layers.FirstOrDefault(layerPart => String.Equals(layerPart.Name, layer, StringComparison.OrdinalIgnoreCase));
        }

        private BlogPart GetBlog(int blogId, string blogPath) {
            return _contentManager.Get<BlogPart>(blogId) ?? _blogService.Get(blogPath);
        }
    }
}