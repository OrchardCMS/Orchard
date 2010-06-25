using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Events;

namespace Orchard.ContentManagement.MetaData {
    public interface IContentDefinitionEditorEvents : IEventHandler {
        IEnumerable<TemplateViewModel> TypeEditor(ContentTypeDefinition definition);
        IEnumerable<TemplateViewModel> TypePartEditor(ContentTypeDefinition.Part definition);

        IEnumerable<TemplateViewModel> TypeEditorUpdate(ContentTypeDefinitionBuilder builder, IUpdateModel updateModel);
        IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypeDefinitionBuilder.PartConfigurer builder, IUpdateModel updateModel);
    }

    public abstract class ContentDefinitionEditorEventsBase : IContentDefinitionEditorEvents {
        public virtual IEnumerable<TemplateViewModel> TypeEditor(ContentTypeDefinition definition) {
            return Enumerable.Empty<TemplateViewModel>();
        }

        public virtual IEnumerable<TemplateViewModel> TypePartEditor(ContentTypeDefinition.Part definition) {
            return Enumerable.Empty<TemplateViewModel>();
        }

        public virtual IEnumerable<TemplateViewModel> TypeEditorUpdate(ContentTypeDefinitionBuilder builder, IUpdateModel updateModel) {
            return Enumerable.Empty<TemplateViewModel>();
        }

        public virtual IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypeDefinitionBuilder.PartConfigurer builder, IUpdateModel updateModel) {
            return Enumerable.Empty<TemplateViewModel>();
        }

        protected static TemplateViewModel DefinitionTemplate<TModel>(TModel model) {
            return new TemplateViewModel(model, typeof(TModel).Name) {
                TemplateName = "DefinitionTemplates/" + typeof(TModel).Name
            };
        }
    }
}