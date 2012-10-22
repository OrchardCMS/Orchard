using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Alias;
using Orchard.Autoroute.Models;
using Orchard.Autoroute.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Autoroute.ViewModels;
using Orchard.Autoroute.Settings;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;

namespace Orchard.Autoroute.Drivers {
    public class AutoroutePartDriver : ContentPartDriver<AutoroutePart> {
        private readonly IAliasService _aliasService;
        private readonly IContentManager _contentManager;
        private readonly IAutorouteService _autorouteService;
        private readonly IAuthorizer _authorizer;
        private readonly INotifier _notifier;

        public AutoroutePartDriver(
            IAliasService aliasService, 
            IContentManager contentManager,
            IAutorouteService autorouteService,
            IAuthorizer authorizer,
            INotifier notifier) {
            _aliasService = aliasService;
            _contentManager = contentManager;
            _autorouteService = autorouteService;
            _authorizer = authorizer;
            _notifier = notifier;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "Autoroute"; }}

        protected override DriverResult Editor(AutoroutePart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(AutoroutePart part, IUpdateModel updater, dynamic shapeHelper) {

            var settings = part.TypePartDefinition.Settings.GetModel<AutorouteSettings>();
            
            // if the content type has no pattern for autoroute, then use a default one
            if(!settings.Patterns.Any()) {
                settings.AllowCustomPattern = true;
                settings.AutomaticAdjustmentOnEdit = false;
                settings.DefaultPatternIndex = 0;
                settings.Patterns = new List<RoutePattern> {new RoutePattern {Name = "Title", Description = "my-title", Pattern = "{Content.Slug}"}};

                _notifier.Warning(T("No route patterns are currently defined for this Content Type. If you don't set one in the settings, a default one will be used."));
            }

            var viewModel = new AutoroutePartEditViewModel {
                CurrentUrl = part.DisplayAlias,
                Settings = settings
            };

            // retrieve home page
            var homepage = _aliasService.Get(string.Empty);
            var displayRouteValues = _contentManager.GetItemMetadata(part).DisplayRouteValues;

            if(homepage.Match(displayRouteValues)) {
                viewModel.PromoteToHomePage = true;
            }

            if (settings.PerItemConfiguration) {
                // if enabled, the list of all available patterns is displayed, and the user can 
                // select which one to use

                // todo: later
            }

            var previous = part.DisplayAlias;
            if (updater != null && updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                
                // remove any trailing slash in the permalink
                while(!string.IsNullOrEmpty(viewModel.CurrentUrl) && viewModel.CurrentUrl.StartsWith("/")) {
                    viewModel.CurrentUrl = viewModel.CurrentUrl.Substring(1);
                }

                part.DisplayAlias = viewModel.CurrentUrl;

                // reset the alias if we need to force regeneration, and the user didn't provide a custom one
                if(settings.AutomaticAdjustmentOnEdit && previous == part.DisplayAlias) {
                    part.DisplayAlias = string.Empty;
                }

                if (!_autorouteService.IsPathValid(part.DisplayAlias)) {
                    var path = (part.DisplayAlias ?? String.Empty);
                    if (path.StartsWith(".") || path.EndsWith("."))
                        updater.AddModelError("CurrentUrl", T("The \".\" can't be used at either end of the permalink."));
                    else
                        updater.AddModelError("CurrentUrl", T("Please do not use any of the following characters in your permalink: \":\", \"?\", \"#\", \"[\", \"]\", \"@\", \"!\", \"$\", \"&\", \"'\", \"(\", \")\", \"*\", \"+\", \",\", \";\", \"=\", \", \"<\", \">\", \"\\\", \"|\". No spaces are allowed (please use dashes or underscores instead)."));
                }

                // if CurrentUrl is set, the handler won't try to create an alias for it
                // but instead keep the value

                // if home page is requested, use "/" to have the handler create a homepage alias
                if(_authorizer.Authorize(Permissions.SetHomePage) && viewModel.PromoteToHomePage) {
                    part.DisplayAlias = "/";
                }
            }

            return ContentShape("Parts_Autoroute_Edit", 
                () => shapeHelper.EditorTemplate(TemplateName: "Parts.Autoroute.Edit", Model: viewModel, Prefix: Prefix));
        }

        protected override void Importing(AutoroutePart part, ContentManagement.Handlers.ImportContentContext context) {
            var displayAlias = context.Attribute(part.PartDefinition.Name, "Alias");
            if (displayAlias != null) {
                part.DisplayAlias = displayAlias;
            }

            var customPattern = context.Attribute(part.PartDefinition.Name, "CustomPattern");
            if (customPattern != null) {
                part.CustomPattern = customPattern;
            }

            var useCustomPattern = context.Attribute(part.PartDefinition.Name, "UseCustomPattern");
            if (useCustomPattern != null) {
                part.UseCustomPattern = bool.Parse(useCustomPattern);
            }
        }

        protected override void Exporting(AutoroutePart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Alias", String.IsNullOrEmpty(part.Record.DisplayAlias) ? "/" : part.Record.DisplayAlias);
            context.Element(part.PartDefinition.Name).SetAttributeValue("CustomPattern", part.Record.CustomPattern);
            context.Element(part.PartDefinition.Name).SetAttributeValue("UseCustomPattern", part.Record.UseCustomPattern);
        }
    }
}
