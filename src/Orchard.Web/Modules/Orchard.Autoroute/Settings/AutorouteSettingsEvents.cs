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

            //Adding null Patterns for the ability to add another Pattern in the UI
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
                //we add a new empty line for each culture
                newPatterns.Add(new RoutePattern { Culture = culture, Name = null, Description = null, Pattern = null });

                // if the content type has no defaultPattern for autoroute, then use a default one
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
                // remove empty patterns
                var patterns = settings.Patterns;
                patterns.RemoveAll(p => String.IsNullOrWhiteSpace(p.Name) && String.IsNullOrWhiteSpace(p.Pattern) && String.IsNullOrWhiteSpace(p.Description));

                //If there is no default pattern for each culture we set default ones
                List<RoutePattern> newPatterns = new List<RoutePattern>();
                int current = 0;
                foreach (string culture in settings.SiteCultures) {
                    if (settings.Patterns.Any(x => x.Culture == culture)) {
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
