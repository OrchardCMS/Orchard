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

            //if a culture is not set on the pattern we set it to the default site culture for backward compatibility
            if (!settings.Patterns.Any(x => String.Equals(x.Culture, settings.DefaultSiteCulture, StringComparison.OrdinalIgnoreCase))) {
                foreach (RoutePattern pattern in settings.Patterns.Where(x => String.IsNullOrWhiteSpace(x.Culture))) {
                    settings.Patterns.Where(x => x.GetHashCode() == pattern.GetHashCode()).FirstOrDefault().Culture = settings.DefaultSiteCulture;
                }
            }

            //Adding Patterns for the UI
            List<RoutePattern> newPatterns = new List<RoutePattern>();
            int current = 0;
            foreach (string culture in settings.SiteCultures) {
                foreach (RoutePattern routePattern in settings.Patterns.Where(x => String.Equals(x.Culture, culture, StringComparison.OrdinalIgnoreCase))) {
                    if (settings.Patterns.Any(x => String.Equals(x.Culture, culture, StringComparison.OrdinalIgnoreCase))) {
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
                if (!settings.Patterns.Where(x => String.Equals(x.Culture, culture, StringComparison.OrdinalIgnoreCase)).Any()) {
                    newPatterns.Add(new RoutePattern { Culture = culture, Name = "Title", Description = "my-title", Pattern = "{Content.Slug}" });
                }

                //we add a new empty line for each culture
                newPatterns.Add(new RoutePattern { Culture = culture, Name = null, Description = null, Pattern = null });

                // if the content type has no defaultPattern for autoroute, then assign one
                if (!settings.DefaultPatterns.Any(x => String.Equals(x.Culture, culture, StringComparison.OrdinalIgnoreCase))) {
                    //if we are in the default culture check the old setting
                    if (String.Equals(culture, _cultureManager.GetSiteCulture(), StringComparison.OrdinalIgnoreCase)) {
                        var defaultPatternIndex = settings.DefaultPatternIndex;
                        if (!String.IsNullOrWhiteSpace(defaultPatternIndex)) {
                            var patternIndex = defaultPatternIndex;
                            settings.DefaultPatterns.Add(new DefaultPattern { Culture = settings.DefaultSiteCulture, PatternIndex = patternIndex });
                        } else {
                            settings.DefaultPatterns.Add(new DefaultPattern { PatternIndex = "0", Culture = culture });
                        }
                    } else {
                        settings.DefaultPatterns.Add(new DefaultPattern { PatternIndex = "0", Culture = culture });
                    }
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
                        foreach (RoutePattern routePattern in settings.Patterns.Where(x => String.Equals(x.Culture, culture, StringComparison.OrdinalIgnoreCase))) {
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
