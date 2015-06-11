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

namespace Orchard.Autoroute.Settings {
    public class AutorouteSettingsHooks : ContentDefinitionEditorEventsBase {
        private readonly INotifier _notifier;

        public AutorouteSettingsHooks(INotifier notifier) {
            _notifier = notifier;
        }

        public Localizer T { get; set; }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "AutoroutePart")
                yield break;

            var settings = definition.Settings.GetModel<AutorouteSettings>();

            // add an empty pattern for the editor
            settings.Patterns.Add(new RoutePattern());

            yield return DefinitionTemplate(settings);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "AutoroutePart")
                yield break;

            var settings = new AutorouteSettings {
                Patterns = new List<RoutePattern>()
            };

            if (updateModel.TryUpdateModel(settings, "AutorouteSettings", null, null)) {

                var defaultPattern = settings.Patterns[settings.DefaultPatternIndex];
                // remove empty patterns
                var patterns = settings.Patterns;
                patterns.RemoveAll(p => String.IsNullOrWhiteSpace(p.Name) && String.IsNullOrWhiteSpace(p.Pattern) && String.IsNullOrWhiteSpace(p.Description));

                if (patterns.Count == 0) {
                    patterns.Add(new RoutePattern {
                        Name = "Title",
                        Description = "my-title",
                        Pattern = "{Content.Slug}"
                    });

                    _notifier.Warning(T("A default pattern has been added to AutoroutePart"));
                }

                settings.Patterns = patterns;
                // search for the pattern which was marked as default, and update its index
                settings.DefaultPatternIndex = patterns.IndexOf(defaultPattern);

                // if a wrong pattern was selected and there is at least one pattern, default to first
                if (settings.DefaultPatternIndex == -1 && settings.Patterns.Any()) {
                    settings.DefaultPatternIndex = 0;
                }

                // update the settings builder
                settings.Build(builder);
            }

            yield return DefinitionTemplate(settings);
        }
    }
}
