using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Descriptor.Models;

namespace TinyMce.Settings {
    public class EditorEvents : ContentDefinitionEditorEventsBase {

        private string[] _htmlParts = new string[] { "BodyPart", "LayoutPart" };
        private string[] _htmlFields = new string[] { "TextField" };
        private bool _contentLinksDependenciesEnabled = false;

        public EditorEvents(ShellDescriptor shellDescriptor) {
            var contenPickerEnabled = shellDescriptor.Features.Any(x => x.Name == "Orchard.ContentPicker") ? true : false;
            var tokensHtmlFilterEnabled = shellDescriptor.Features.Any(x => x.Name == "Orchard.Tokens.HtmlFilter") ? true : false;
            _contentLinksDependenciesEnabled = contenPickerEnabled && tokensHtmlFilterEnabled;
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (!_contentLinksDependenciesEnabled || !_htmlFields.Any(x => x.Equals(definition.FieldDefinition.Name, StringComparison.InvariantCultureIgnoreCase)))
                yield break;
            var model = definition.Settings.GetModel<ContentLinksSettings>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (!_contentLinksDependenciesEnabled || !_htmlFields.Any(x => x.Equals(builder.FieldType, StringComparison.InvariantCultureIgnoreCase)))
                yield break;

            var model = new ContentLinksSettings();
            updateModel.TryUpdateModel(model, "ContentLinksSettings", null, null);
            builder.WithSetting("ContentLinksSettings.DisplayedContentTypes", model.DisplayedContentTypes);

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (!_contentLinksDependenciesEnabled || !_htmlParts.Any(x => x.Equals(definition.PartDefinition.Name, StringComparison.InvariantCultureIgnoreCase)))
                yield break;
            var model = definition.Settings.GetModel<ContentLinksSettings>();
            yield return DefinitionTemplate(model);
        }


        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (!_contentLinksDependenciesEnabled || !_htmlParts.Any(x => x.Equals(builder.Name, StringComparison.InvariantCultureIgnoreCase)))
                yield break;
            var model = new ContentLinksSettings();
            updateModel.TryUpdateModel(model, "ContentLinksSettings", null, null);
            builder.WithSetting("ContentLinksSettings.DisplayedContentTypes", model.DisplayedContentTypes);
            yield return DefinitionTemplate(model);
        }
    }
}
