using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Core.Containers.Settings {
    public class ContainerPartSettings {
        public const int PageSizeDefaultDefault = 10;
        public const bool PaginatedDefaultDefault = true;

        private int? _pageSizeDefault;
        private bool? _paginiatedDefault;

        public int PageSizeDefault {
            get {
                return _pageSizeDefault != null
                         ? (int)_pageSizeDefault
                         : PageSizeDefaultDefault;
            }
            set { _pageSizeDefault = value; }
        }

        public bool PaginatedDefault {
            get {
                return _paginiatedDefault != null
                         ? (bool)_paginiatedDefault
                         : PaginatedDefaultDefault;
            }
            set { _paginiatedDefault = value; }
        }
    }

    public class ContainerTypePartSettings {
        public int? PageSizeDefault { get; set; }
        public bool? PaginatedDefault { get; set; }
    }

    public class ContainerSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "ContainerPart")
                yield break;

            var model = definition.Settings.GetModel<ContainerTypePartSettings>();
            var partModel = definition.PartDefinition.Settings.GetModel<ContainerPartSettings>();

            if (model.PageSizeDefault == null)
                model.PageSizeDefault = partModel.PageSizeDefault;

            if (model.PaginatedDefault == null)
                model.PaginatedDefault = partModel.PaginatedDefault;

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> PartEditor(ContentPartDefinition definition) {
            if (definition.Name != "ContainerPart")
                yield break;

            var model = definition.Settings.GetModel<ContainerPartSettings>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "ContainerPart")
                yield break;

            var model = new ContainerTypePartSettings();
            updateModel.TryUpdateModel(model, "ContainerTypePartSettings", null, null);
            builder.WithSetting("ContainerTypePartSettings.PageSizeDefault", model.PageSizeDefault.ToString());
            builder.WithSetting("ContainerTypePartSettings.PaginatedDefault", model.PaginatedDefault.ToString());
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> PartEditorUpdate(ContentPartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "ContainerPart")
                yield break;

            var model = new ContainerPartSettings();
            updateModel.TryUpdateModel(model, "ContainerPartSettings", null, null);
            builder.WithSetting("ContainerPartSettings.PageSizeDefault", model.PageSizeDefault.ToString());
            builder.WithSetting("ContainerPartSettings.PaginatedDefault", model.PaginatedDefault.ToString());
            yield return DefinitionTemplate(model);
        }
    }
}
