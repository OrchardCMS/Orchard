using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Settings.Models;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;
using Orchard.Logging;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;

namespace Orchard.Widgets.Drivers {
    public class ColumnElementDriver : ElementDriver<Column> {
        private readonly IOrchardServices _orchardServices;
        private readonly IRuleManager _ruleManager;
        private readonly IWidgetsService _widgetsService;

        public ColumnElementDriver(IOrchardServices orchardServices, IRuleManager ruleManager, IWidgetsService widgetsService) {
            _orchardServices = orchardServices;
            _ruleManager = ruleManager;
            _widgetsService = widgetsService;
        }

        protected override void OnLayoutSaving(Column element, ElementSavingContext context) {
            ValidateZoneName(element, context);
        }

        protected override void OnDisplaying(Column element, ElementDisplayingContext context) {
            RenderWidgets(element, context);
        }

        private void ValidateZoneName(Column element, ElementSavingContext context) {
            if (String.IsNullOrWhiteSpace(element.ZoneName))
                return; // Nothing to validate.

            if (element.IsTemplated)
                return; // No need to validate templated columns.

            var blacklist = new HashSet<string>();

            // Add theme zones to the blacklist.
            var themeZones = _widgetsService.GetZones();
            Add(blacklist, themeZones);

            // Add any zones from the current layout (except the zone name of the current column) to the blacklist.
            var siblingColumns = context.Elements.Flatten().Where(x => x is Column && x != element).Cast<Column>().ToList();
            var siblingZones = siblingColumns.Where(x => !String.IsNullOrWhiteSpace(x.ZoneName)).Select(x => x.ZoneName);
            Add(blacklist, siblingZones);

            // Check if the specified zone is blacklisted.
            if (blacklist.Contains(element.ZoneName))
                context.Updater.AddModelError("ZoneName", T("The zone name '{0}' is already in use.", element.ZoneName));
        }

        private void RenderWidgets(Column element, ElementDisplayingContext context) {
            var zoneName = element.ZoneName;

            if (String.IsNullOrWhiteSpace(zoneName))
                return;

            var activeLayers = _orchardServices.ContentManager.Query<LayerPart>().ForType("Layer").List();
            var activeLayerIds = new List<int>();

            foreach (var activeLayer in activeLayers) {
                try {
                    if (_ruleManager.Matches(activeLayer.LayerRule)) {
                        activeLayerIds.Add(activeLayer.ContentItem.Id);
                    }
                }
                catch (Exception e) {
                    Logger.Warning(e, T("An error occured during layer evaluation on: {0}", activeLayer.Name).Text);
                }
            }

            var widgetParts = _widgetsService.GetWidgets(layerIds: activeLayerIds.ToArray()).Where(x => x.Zone == zoneName);
            var defaultCulture = _orchardServices.WorkContext.CurrentSite.As<SiteSettingsPart>().SiteCulture;
            var currentCulture = _orchardServices.WorkContext.CurrentCulture;

            foreach (var widgetPart in widgetParts) {
                var commonPart = widgetPart.As<ICommonPart>();
                if (commonPart == null || commonPart.Container == null) {
                    Logger.Warning("The widget '{0}' is has no assigned layer or the layer does not exist.", widgetPart.Title);
                    continue;
                }

                // Ignore widget for different cultures,
                var localizablePart = widgetPart.As<ILocalizableAspect>();
                if (localizablePart != null) {
                    // If localized culture is null then show if current culture is the default
                    // this allows a user to show a content item for the default culture only.
                    if (localizablePart.Culture == null && defaultCulture != currentCulture) {
                        continue;
                    }

                    // If culture is set, show only if current culture is the same.
                    if (localizablePart.Culture != null && localizablePart.Culture != currentCulture) {
                        continue;
                    }
                }

                // Check permissions.
                if (!_orchardServices.Authorizer.Authorize(Core.Contents.Permissions.ViewContent, widgetPart)) {
                    continue;
                }

                var widgetShape = _orchardServices.ContentManager.BuildDisplay(widgetPart);

                context.ElementShape.Add(widgetShape, widgetPart.Position);
            }
        }

        private static void Add(ISet<string> set, IEnumerable<string> zones) {
            foreach (var zone in zones) {
                set.Add(zone);
            }
        }
    }
}