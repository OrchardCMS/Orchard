using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Alias;
using Orchard.Autoroute.Models;
using Orchard.Autoroute.Services;
using Orchard.Autoroute.Settings;
using Orchard.Autoroute.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;

namespace Orchard.Autoroute.Drivers {
    public class AutoroutePartDriver : ContentPartDriver<AutoroutePart> {
        private readonly IAutorouteService _autorouteService;
        private readonly INotifier _notifier;
        private readonly IHomeAliasService _homeAliasService;
        private readonly IAliasService _aliasService;
        private readonly ICultureManager _cultureManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IContentManager _contentManager;

        public AutoroutePartDriver(
            IAutorouteService autorouteService,
            INotifier notifier, 
            IHomeAliasService homeAliasService,
            IAliasService aliasService,
            IAuthorizer authorizer,
            ICultureManager cultureManager,
            IContentManager contentManager,
            IHttpContextAccessor httpContextAccessor) {

            _aliasService = aliasService;
            _contentManager = contentManager;
            _autorouteService = autorouteService;
            _notifier = notifier;
            _homeAliasService = homeAliasService;
            _cultureManager = cultureManager;
            _httpContextAccessor = httpContextAccessor;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(AutoroutePart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(AutoroutePart part, IUpdateModel updater, dynamic shapeHelper) {
            var settings = part.TypePartDefinition.Settings.GetModel<AutorouteSettings>();
            var itemCulture = _cultureManager.GetSiteCulture();
            
            // If we are editing an existing content item, check to see if we are an ILocalizableAspect so we can use its culture for alias generation.
            if (part.Record.Id != 0) {
                var localizableAspect = part.As<ILocalizableAspect>();

                if (localizableAspect != null) {
                    itemCulture = localizableAspect.Culture;
                }
            }

            if (settings.UseCulturePattern) {
                // Hack: if the LocalizedPart is attached to the content item, it will submit the following form value,
                // which we use to determine what culture to use for alias generation.
                var context = _httpContextAccessor.Current();
                if (!String.IsNullOrEmpty(context.Request.Form["Localization.SelectedCulture"])) {
                    itemCulture = context.Request.Form["Localization.SelectedCulture"].ToString();
                }
            }

            // We update the settings assuming that when 
            // pattern culture = null or "" it means culture = default website culture
            // for patterns that we migrated.
            foreach (var pattern in settings.Patterns.Where(x => String.IsNullOrWhiteSpace(x.Culture))) {
                pattern.Culture = _cultureManager.GetSiteCulture(); ;
            }

            // We do the same for default patterns.
            foreach (var pattern in settings.DefaultPatterns.Where(x => String.IsNullOrWhiteSpace(x.Culture))) {
                pattern.Culture = _cultureManager.GetSiteCulture();
            }
            
            // If the content type has no pattern for autoroute, then use a default one.
            if (!settings.Patterns.Any(x => String.Equals(x.Culture, itemCulture, StringComparison.OrdinalIgnoreCase))) {
                settings.Patterns = new List<RoutePattern> { new RoutePattern { Name = "Title", Description = "my-title", Pattern = "{Content.Slug}", Culture = itemCulture } };
            }

            // If the content type has no defaultPattern for autoroute, then use a default one.
            if (!settings.DefaultPatterns.Any(x => String.Equals(x.Culture, itemCulture, StringComparison.OrdinalIgnoreCase))) {
                // If we are in the default culture, check the old setting.
                if (String.Equals(itemCulture, _cultureManager.GetSiteCulture(), StringComparison.OrdinalIgnoreCase)) {
                    if (!String.IsNullOrWhiteSpace(settings.DefaultPatternIndex)) {
                        var patternIndex = settings.DefaultPatternIndex;
                        settings.DefaultPatterns.Add(new DefaultPattern { PatternIndex = patternIndex, Culture = itemCulture });
                    } else {
                        settings.DefaultPatterns.Add(new DefaultPattern { PatternIndex = "0", Culture = itemCulture });
                    }
                } else {
                    settings.DefaultPatterns.Add(new DefaultPattern { PatternIndex = "0", Culture = itemCulture });
                }
            }

            var viewModel = new AutoroutePartEditViewModel {
                CurrentUrl = part.DisplayAlias,
                Settings = settings,
                CurrentCulture = itemCulture
            };

            // Retrieve home page.
            var homePageId = _homeAliasService.GetHomePageId(VersionOptions.Latest);
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
                
                if (part.DisplayAlias != null && part.DisplayAlias.Length > 1850){
                    updater.AddModelError("CurrentUrl", T("Your permalink is too long. The permalink can only be up to 1,850 characters."));
                }

                // Mark the content item to be the homepage. Once this content isp ublished, the home alias will be updated to point to this content item.
                part.PromoteToHomePage = viewModel.PromoteToHomePage;
            }

            return ContentShape("Parts_Autoroute_Edit", 
                () => shapeHelper.EditorTemplate(TemplateName: "Parts.Autoroute.Edit", Model: viewModel, Prefix: Prefix));
        }

        protected override void Importing(AutoroutePart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "Alias", s => part.DisplayAlias = s);
            context.ImportAttribute(part.PartDefinition.Name, "CustomPattern", s => part.CustomPattern = s);
            context.ImportAttribute(part.PartDefinition.Name, "UseCustomPattern", s => part.UseCustomPattern = XmlHelper.Parse<bool>(s));
            context.ImportAttribute(part.PartDefinition.Name, "UseCulturePattern", s => part.UseCulturePattern = XmlHelper.Parse<bool>(s));
            context.ImportAttribute(part.PartDefinition.Name, "PromoteToHomePage", s => part.PromoteToHomePage = XmlHelper.Parse<bool>(s));
        }

        protected override void Exporting(AutoroutePart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Alias", part.Record.DisplayAlias);
            context.Element(part.PartDefinition.Name).SetAttributeValue("CustomPattern", part.Record.CustomPattern);
            context.Element(part.PartDefinition.Name).SetAttributeValue("UseCustomPattern", part.Record.UseCustomPattern);
            context.Element(part.PartDefinition.Name).SetAttributeValue("UseCulturePattern", part.Record.UseCulturePattern);
            context.Element(part.PartDefinition.Name).SetAttributeValue("PromoteToHomePage", part.PromoteToHomePage);
        }
    }
}
