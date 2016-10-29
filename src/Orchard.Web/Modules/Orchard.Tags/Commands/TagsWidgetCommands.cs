using System;
using System.Linq;
using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Tags.Models;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;

namespace Orchard.Tags.Commands {
    public class TagWidgetCommands : DefaultOrchardCommandHandler {
        private readonly IWidgetsService _widgetsService;
        private readonly ISiteService _siteService;
        private readonly IMembershipService _membershipService;
        private readonly IContentManager _contentManager;

        public TagWidgetCommands(
            IWidgetsService widgetsService, 
            ISiteService siteService, 
            IMembershipService membershipService,
            IContentManager contentManager) {
            _widgetsService = widgetsService;
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
        public string Slug { get; set; }

        [OrchardSwitch]
        public string Buckets { get; set; }

        [CommandName("tags widget create tagcloud")]
        [CommandHelp("tags widget create tagcloud /Title:<title> /Name:<name> /Zone:<zone> /Position:<position> /Layer:<layer> [/Identity:<identity>] [/RenderTitle:true|false] [/Owner:<owner>] [/Slug:<slug>] [/Buckets:<number>]\r\n\t" + "Creates a new widget")]
        [OrchardSwitches("Title,Name,Zone,Position,Layer,Buckets,Identity,Owner,RenderTitle,Slug")]
        public void CreateTagsCloudWidget() {
            var type = "TagCloud";

            var layer = GetLayer(Layer);
            if (layer == null) {
                Context.Output.WriteLine(T("Creating {0} widget failed: layer {1} was not found.", type, Layer));
                return;
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

            if (widget == null) {
                return;
            }

            widget.As<TagCloudPart>().Slug = Slug;

            // It's an optional parameter and defaults to 5.
            if (!string.IsNullOrWhiteSpace(Buckets)) {
                int BucketsAsNumber = 0;
                if (Int32.TryParse(Buckets, out BucketsAsNumber)) {
                    widget.As<TagCloudPart>().Buckets = BucketsAsNumber;
                }
            }

            _contentManager.Publish(widget.ContentItem);
            Context.Output.WriteLine(T("{0} widget created successfully.", type).Text);
        }

        private LayerPart GetLayer(string layer) {
            var layers = _widgetsService.GetLayers();
            return layers.FirstOrDefault(layerPart => String.Equals(layerPart.Name, layer, StringComparison.OrdinalIgnoreCase));
        }        
    }
}