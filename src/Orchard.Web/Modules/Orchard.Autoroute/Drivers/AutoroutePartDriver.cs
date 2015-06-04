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
using Orchard.Localization.Services;
using Orchard.Localization.Models;
using Orchard.Mvc;
using System.Web;
using Orchard.ContentManagement.Aspects;

namespace Orchard.Autoroute.Drivers {
    public class AutoroutePartDriver : ContentPartDriver<AutoroutePart> {
        private readonly IAliasService _aliasService;
        private readonly IContentManager _contentManager;
        private readonly IAutorouteService _autorouteService;
        private readonly IAuthorizer _authorizer;
        private readonly INotifier _notifier;
        private readonly ICultureManager _cultureManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AutoroutePartDriver(
            IAliasService aliasService,
            IContentManager contentManager,
            IAutorouteService autorouteService,
            IAuthorizer authorizer,
            INotifier notifier,
            ICultureManager cultureManager,
            IHttpContextAccessor httpContextAccessor) {
            _aliasService = aliasService;
            _contentManager = contentManager;
            _autorouteService = autorouteService;
            _authorizer = authorizer;
            _notifier = notifier;
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

            //if we are editing an existing content item
            if (part.Record.Id != 0) {
                ContentItem contentItem = _contentManager.Get(part.Record.ContentItemRecord.Id);
                var aspect = contentItem.As<ILocalizableAspect>();

                if (aspect != null) {
                    itemCulture = aspect.Culture;
                }
            }

            if (settings.UseCulturePattern) {
                //if we are creating from a form post we use the form value for culture
                HttpContextBase context = _httpContextAccessor.Current();
                if (context.Request.Form["Localization.SelectedCulture"] != null) {
                    itemCulture = context.Request.Form["Localization.SelectedCulture"].ToString();
                }
            }

            // if the content type has no pattern for autoroute, then use a default one
            if (!settings.Patterns.Any(x => x.Culture == itemCulture)) {
                settings.AllowCustomPattern = true;
                settings.AutomaticAdjustmentOnEdit = false;
                settings.Patterns = new List<RoutePattern> { new RoutePattern { Name = "Title", Description = "my-title", Pattern = "{Content.Slug}", Culture = itemCulture } };

                _notifier.Warning(T("No route patterns are currently defined for this Content Type. If you don't set one in the settings, a default one will be used."));
            }

            // if the content type has no defaultPattern for autoroute, then use a default one
            if (!settings.DefaultPatterns.Any(x => x.Culture == itemCulture)) {
                settings.DefaultPatterns = new List<DefaultPattern> { new DefaultPattern { PatternIndex = "0", Culture = itemCulture } };
            }

            var viewModel = new AutoroutePartEditViewModel {
                CurrentUrl = part.DisplayAlias,
                Settings = settings,
                CurrentCulture = itemCulture
            };

            // retrieve home page
            var homepage = _aliasService.Get(string.Empty);
            var displayRouteValues = _contentManager.GetItemMetadata(part).DisplayRouteValues;

            if (homepage.Match(displayRouteValues)) {
                viewModel.PromoteToHomePage = true;
            }

            if (settings.PerItemConfiguration) {
                // if enabled, the list of all available patterns is displayed, and the user can 
                // select which one to use

                // todo: later
            }

            var previous = part.DisplayAlias;
            if (updater != null && updater.TryUpdateModel(viewModel, Prefix, null, null)) {

                // remove any leading slash in the permalink
                if (viewModel.CurrentUrl != null) {
                    viewModel.CurrentUrl = viewModel.CurrentUrl.TrimStart('/');
                }

                part.DisplayAlias = viewModel.CurrentUrl;

                // reset the alias if we need to force regeneration, and the user didn't provide a custom one
                if (settings.AutomaticAdjustmentOnEdit && previous == part.DisplayAlias) {
                    part.DisplayAlias = string.Empty;
                }

                if (!_autorouteService.IsPathValid(part.DisplayAlias)) {
                    updater.AddModelError("CurrentUrl", T("Please do not use any of the following characters in your permalink: \":\", \"?\", \"#\", \"[\", \"]\", \"@\", \"!\", \"$\", \"&\", \"'\", \"(\", \")\", \"*\", \"+\", \",\", \";\", \"=\", \", \"<\", \">\", \"\\\", \"|\", \"%\", \".\". No spaces are allowed (please use dashes or underscores instead)."));
                }

                // if CurrentUrl is set, the handler won't try to create an alias for it
                // but instead keep the value

                // if home page is requested, use "/" to have the handler create a homepage alias
                if (viewModel.PromoteToHomePage) {
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

            var useCulturePattern = context.Attribute(part.PartDefinition.Name, "UseCulturePattern");
            if (useCulturePattern != null) {
                part.UseCulturePattern = bool.Parse(useCulturePattern);
            }
        }

        protected override void Exporting(AutoroutePart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Alias", String.IsNullOrEmpty(part.Record.DisplayAlias) ? "/" : part.Record.DisplayAlias);
            context.Element(part.PartDefinition.Name).SetAttributeValue("CustomPattern", part.Record.CustomPattern);
            context.Element(part.PartDefinition.Name).SetAttributeValue("UseCustomPattern", part.Record.UseCustomPattern);
            context.Element(part.PartDefinition.Name).SetAttributeValue("UseCulturePattern", part.Record.UseCulturePattern);
        }
    }
}
