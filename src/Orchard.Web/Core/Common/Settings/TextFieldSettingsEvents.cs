using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Core.Common.ViewModels;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Utility.Extensions;

namespace Orchard.Core.Common.Settings {
    public class TextFieldSettingsEvents : ContentDefinitionEditorEventsBase {
        private readonly IOrchardServices _orchardServices;
        private readonly Func<IShapeTableLocator> _shapeTableLocator;

        public TextFieldSettingsEvents(IOrchardServices orchardServices, Func<IShapeTableLocator> shapeTableLocator) {
            _orchardServices = orchardServices;
            _shapeTableLocator = shapeTableLocator;
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "TextField") {
                var shapeTable = _shapeTableLocator().Lookup(_orchardServices.WorkContext.CurrentTheme.Id);
                var flavors = shapeTable.Bindings.Keys
                    .Where(x => x.StartsWith("Body_Editor__", StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.Substring("Body_Editor__".Length))
                    .Where(x => !String.IsNullOrWhiteSpace(x))
                    .Select(x => x[0].ToString(CultureInfo.InvariantCulture).ToUpper() + x.Substring(1) )
                    .Select(x => x.CamelFriendly())
                    ;


                var model = new TextFieldSettingsEventsViewModel {
                    Settings = definition.Settings.GetModel<TextFieldSettings>(),
                    Flavors = flavors.ToArray()
                };
                    
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "TextField") {
                yield break;
            }

            var model = new TextFieldSettingsEventsViewModel {
                Settings = new TextFieldSettings()
            };

            if (updateModel.TryUpdateModel(model, "TextFieldSettingsEventsViewModel", null, null)) {
                builder.WithSetting("TextFieldSettings.Flavor", model.Settings.Flavor);
                builder.WithSetting("TextFieldSettings.Hint", model.Settings.Hint);
                builder.WithSetting("TextFieldSettings.Required", model.Settings.Required.ToString(CultureInfo.InvariantCulture));

                yield return DefinitionTemplate(model);
            }
        }
    }
}