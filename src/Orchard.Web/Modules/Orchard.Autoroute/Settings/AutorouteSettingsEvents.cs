using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Localization;
using Orchard.UI.Notify;
using Orchard.Localization.Services;

namespace Orchard.Autoroute.Settings {
    public class AutorouteSettingsHooks : ContentDefinitionEditorEventsBase {
        private readonly INotifier _notifier;
        private readonly ICultureManager _cultureManager;

        public AutorouteSettingsHooks(INotifier notifier, ICultureManager cultureManager) {
            _notifier = notifier;
            _cultureManager = cultureManager;
        }

        public Localizer T { get; set; }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "AutoroutePart")
                yield break;

            var settings = definition.Settings.GetModel<AutorouteSettings>();

            //get cultures
            settings.SiteCultures = _cultureManager.ListCultures().ToList();
            //get default site culture
            settings.DefaultSiteCulture = _cultureManager.GetSiteCulture();

            //if a culture is not set on the token we set it to the default site culture for backward compatibility
            //NOT SURE ABOUT THIS
            if (!settings.Patterns.Any(x => x.Culture == settings.DefaultSiteCulture)) {
                foreach (RoutePattern pattern in settings.Patterns.Where(x => x.Culture == null)) {
                    settings.Patterns.Where(x => x.GetHashCode() == pattern.GetHashCode()).FirstOrDefault().Culture = settings.DefaultSiteCulture;
                }
            }

            //Adding Patterns for the UI
            List<RoutePattern> newPatterns = new List<RoutePattern>();
            int current = 0;
            foreach (string culture in settings.SiteCultures) {
                foreach (RoutePattern routePattern in settings.Patterns.Where(x => x.Culture == culture)) {
                    if (settings.Patterns.Any(x => x.Culture == culture)) {
                        newPatterns.Add(settings.Patterns[current]);
                    } else {
                        newPatterns.Add(new RoutePattern {
                            Name = "Title",
                            Description = "my-title",
                            Pattern = "{Content.Slug}",
                            Culture = settings.DefaultSiteCulture
                        });
                    }
                    current++;
                }

                //We add a pattern for each culture if there is none
                if (!settings.Patterns.Where(x => x.Culture == culture).Any()) {
                    //We add the default pattern from migrations
                    if (settings.Patterns.Where(x => String.IsNullOrEmpty(x.Culture)).Any()) {
                        //we add the RoutePattern and we set the culture since there is none defined
                        RoutePattern migrationRoutePattern = settings.Patterns.Where(x => String.IsNullOrEmpty(x.Culture)).First();
                        newPatterns.Add(new RoutePattern { Culture = culture, Name = migrationRoutePattern.Name, Description = migrationRoutePattern.Description, Pattern = migrationRoutePattern.Pattern });
                    } else {
                        //we add the default pattern for custom content types or modules that don't define it in their migration
                        newPatterns.Add(new RoutePattern { Culture = culture, Name = "Title", Description = "my-title", Pattern = "{Content.Slug}" });
                    }
                }

                //we add a new empty line for each culture
                newPatterns.Add(new RoutePattern { Culture = culture, Name = null, Description = null, Pattern = null });

                // if the content type has no defaultPattern for autoroute, then assign a the first one we just created
                if (!settings.DefaultPatterns.Any(x => x.Culture == culture)) {
                    settings.DefaultPatterns.Add(new DefaultPattern { PatternIndex = "0", Culture = culture });
                }
            }

            settings.Patterns = newPatterns;

            yield return DefinitionTemplate(settings);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "AutoroutePart")
                yield break;

            var settings = new AutorouteSettings {
                Patterns = new List<RoutePattern>()
            };

            //get cultures
            settings.SiteCultures = _cultureManager.ListCultures().ToList();

            if (updateModel.TryUpdateModel(settings, "AutorouteSettings", null, null)) {
                //TODO need to add validations client and/or server side here
                // remove empty patterns
                var patterns = settings.Patterns;
                patterns.RemoveAll(p => String.IsNullOrWhiteSpace(p.Name) && String.IsNullOrWhiteSpace(p.Pattern) && String.IsNullOrWhiteSpace(p.Description));

                //If there is no default pattern for each culture we set default ones
                List<RoutePattern> newPatterns = new List<RoutePattern>();
                int current = 0;
                foreach (string culture in settings.SiteCultures) {
                    if (settings.Patterns.Any(x => String.Equals(x.Culture, culture, StringComparison.OrdinalIgnoreCase))) {
                        foreach (RoutePattern routePattern in settings.Patterns.Where(x => x.Culture == culture)) {
                            newPatterns.Add(settings.Patterns[current]);
                            current++;
                        }
                    } else {
                        newPatterns.Add(new RoutePattern {
                            Name = "Title",
                            Description = "my-title",
                            Pattern = "{Content.Slug}",
                            Culture = culture
                        });

                        _notifier.Warning(T("A default pattern has been added to AutoroutePart"));
                    }
                }

                settings.Patterns = newPatterns;

                // update the settings builder
                settings.Build(builder);
            }

            yield return DefinitionTemplate(settings);
        }
    }
}
