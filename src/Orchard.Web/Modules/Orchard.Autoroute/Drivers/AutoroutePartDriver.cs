using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Autoroute.Models;
using Orchard.Autoroute.Services;
using Orchard.Autoroute.Settings;
using Orchard.Autoroute.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.Autoroute.Drivers {
    public class AutoroutePartDriver : ContentPartDriver<AutoroutePart> {
        private readonly IAutorouteService _autorouteService;
        private readonly INotifier _notifier;
        private readonly IHomeAliasService _homeAliasService;

        public AutoroutePartDriver(
            IAutorouteService autorouteService,
            INotifier notifier, 
            IHomeAliasService homeAliasService) {
            
            _autorouteService = autorouteService;
            _notifier = notifier;
            _homeAliasService = homeAliasService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "Autoroute"; }}

        protected override DriverResult Editor(AutoroutePart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(AutoroutePart part, IUpdateModel updater, dynamic shapeHelper) {

            var settings = part.TypePartDefinition.Settings.GetModel<AutorouteSettings>();
            
            // If the content type has no pattern for autoroute, then use a default one.
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

            // Retrieve home page.
            var homePageId = _homeAliasService.GetHomePageId();
            var isHomePage = part.Id == homePageId;

            viewModel.IsHomePage = isHomePage;
            viewModel.PromoteToHomePage = part.PromoteToHomePage;

            if (settings.PerItemConfiguration) {
                // If enabled, the list of all available patterns is displayed, and the user can select which one to use.
                // todo: later
            }

            var previous = part.DisplayAlias;
            if (updater != null && updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                
                // Remove any leading slash in the permalink.
                if (viewModel.CurrentUrl != null) {
                    viewModel.CurrentUrl = viewModel.CurrentUrl.TrimStart('/');
                }

                part.DisplayAlias = viewModel.CurrentUrl;

                // Reset the alias if we need to force regeneration, and the user didn't provide a custom one.
                if(settings.AutomaticAdjustmentOnEdit && previous == part.DisplayAlias) {
                    part.DisplayAlias = String.Empty;
                }

                if (!_autorouteService.IsPathValid(part.DisplayAlias)) {
                    updater.AddModelError("CurrentUrl", T("Please do not use any of the following characters in your permalink: \":\", \"?\", \"#\", \"[\", \"]\", \"@\", \"!\", \"$\", \"&\", \"'\", \"(\", \")\", \"*\", \"+\", \",\", \";\", \"=\", \", \"<\", \">\", \"\\\", \"|\", \"%\", \".\". No spaces are allowed (please use dashes or underscores instead)."));
                }

                // Mark the content item to be the homepage. Once this content isp ublished, the home alias will be updated to point to this content item.
                part.PromoteToHomePage = viewModel.PromoteToHomePage;
            }

            return ContentShape("Parts_Autoroute_Edit",  
                () => shapeHelper.EditorTemplate(TemplateName: "Parts.Autoroute.Edit", Model: viewModel, Prefix: Prefix));
        }

        protected override void Importing(AutoroutePart part, ImportContentContext context) {
            context.ImportAttribute(part.PartDefinition.Name, "Alias", s => part.DisplayAlias = s);
            context.ImportAttribute(part.PartDefinition.Name, "CustomPattern", s => part.CustomPattern = s);
            context.ImportAttribute(part.PartDefinition.Name, "UseCustomPattern", s => part.UseCustomPattern = XmlHelper.Parse<bool>(s));
            context.ImportAttribute(part.PartDefinition.Name, "PromoteToHomePage", s => part.PromoteToHomePage = XmlHelper.Parse<bool>(s));
        }

        protected override void Exporting(AutoroutePart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Alias", part.Record.DisplayAlias);
            context.Element(part.PartDefinition.Name).SetAttributeValue("CustomPattern", part.Record.CustomPattern);
            context.Element(part.PartDefinition.Name).SetAttributeValue("UseCustomPattern", part.Record.UseCustomPattern);
            context.Element(part.PartDefinition.Name).SetAttributeValue("PromoteToHomePage", part.UseCustomPattern);
        }
    }
}
