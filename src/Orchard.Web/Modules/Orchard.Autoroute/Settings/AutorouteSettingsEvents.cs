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

            // Get cultures
            settings.SiteCultures = _cultureManager.ListCultures().ToList();
            // Get default site culture
            settings.DefaultSiteCulture = _cultureManager.GetSiteCulture();

            // Adding Patterns for the UI
            List<RoutePattern> newPatterns = new List<RoutePattern>();

            // Adding a null culture for the culture neutral pattern
            var cultures = new List<string>();
            cultures.Add(null);
            cultures.AddRange(settings.SiteCultures);
            
            foreach (string culture in cultures) {
                // Adding all existing patterns for the culture
                newPatterns.AddRange(
                    settings.Patterns.Where(x => String.Equals(x.Culture, culture, StringComparison.OrdinalIgnoreCase))
                    );

                // Adding a pattern for each culture if there is none
                if (!settings.Patterns.Where(x => String.Equals(x.Culture, culture, StringComparison.OrdinalIgnoreCase)).Any()) {
                    newPatterns.Add(new RoutePattern { Culture = culture, Name = "Title", Description = "my-title", Pattern = "{Content.Slug}" });
                }

                // Adding a new empty line for each culture
                newPatterns.Add(new RoutePattern { Culture = culture, Name = null, Description = null, Pattern = null });

                // If the content type has no defaultPattern for autoroute, assign one
                var defaultPatternExists = false;
                if (String.IsNullOrEmpty(culture))
                    defaultPatternExists = settings.DefaultPatterns.Any(x => String.IsNullOrEmpty(x.Culture));
                else
                    defaultPatternExists = settings.DefaultPatterns.Any(x => String.Equals(x.Culture, culture, StringComparison.OrdinalIgnoreCase));

                if (!defaultPatternExists) {
                    // If in the default culture check the old setting
                    if (String.Equals(culture, _cultureManager.GetSiteCulture(), StringComparison.OrdinalIgnoreCase)) {
                        var defaultPatternIndex = settings.DefaultPatternIndex;
                        if (!String.IsNullOrWhiteSpace(defaultPatternIndex)) {
                            var patternIndex = defaultPatternIndex;
                            settings.DefaultPatterns.Add(new DefaultPattern { Culture = settings.DefaultSiteCulture, PatternIndex = patternIndex });
                        }
                        else {
                            settings.DefaultPatterns.Add(new DefaultPattern { PatternIndex = "0", Culture = culture });
                        }
                    }
                    else {
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

            // Get cultures
            settings.SiteCultures = _cultureManager.ListCultures().ToList();

            if (updateModel.TryUpdateModel(settings, "AutorouteSettings", null, null)) {
                //TODO need to add validations client and/or server side here

                // If some default pattern is an empty pattern set it to the first pattern for the language
                var newDefaultPatterns = new List<DefaultPattern>();

                foreach (var defaultPattern in settings.DefaultPatterns) {
                    RoutePattern correspondingPattern = null;

                    if (string.IsNullOrEmpty(defaultPattern.Culture))
                        correspondingPattern = settings.Patterns.Where(x => String.IsNullOrEmpty(x.Culture)).ElementAt(Convert.ToInt32(defaultPattern.PatternIndex));
                    else
                        correspondingPattern = settings.Patterns.Where(x => String.Equals(x.Culture, defaultPattern.Culture, StringComparison.OrdinalIgnoreCase)).ElementAt(Convert.ToInt32(defaultPattern.PatternIndex));

                    if (String.IsNullOrWhiteSpace(correspondingPattern.Name) && String.IsNullOrWhiteSpace(correspondingPattern.Pattern) && String.IsNullOrWhiteSpace(correspondingPattern.Description))
                        newDefaultPatterns.Add(new DefaultPattern { Culture = defaultPattern.Culture, PatternIndex = "0" });
                    else
                        newDefaultPatterns.Add(defaultPattern);
                }

                settings.DefaultPatterns = newDefaultPatterns;

                // Remove empty patterns
                var patterns = settings.Patterns;
                patterns.RemoveAll(p => String.IsNullOrWhiteSpace(p.Name) && String.IsNullOrWhiteSpace(p.Pattern) && String.IsNullOrWhiteSpace(p.Description));

                // Adding a null culture for the culture neutral pattern
                var cultures = new List<string>();
                cultures.Add(null);
                cultures.AddRange(settings.SiteCultures);

                //If there is no pattern for some culture create a default one
                List<RoutePattern> newPatterns = new List<RoutePattern>();
                int current = 0;
                foreach (string culture in cultures) {
                    if (settings.Patterns.Any(x => String.Equals(x.Culture, culture, StringComparison.OrdinalIgnoreCase))) {
                        foreach (RoutePattern routePattern in settings.Patterns.Where(x => String.Equals(x.Culture, culture, StringComparison.OrdinalIgnoreCase))) {
                            newPatterns.Add(settings.Patterns[current]);
                            current++;
                        }
                    }
                    else {
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

                // Update the settings builder
                settings.Build(builder);
            }

            yield return DefinitionTemplate(settings);
        }
    }
}
