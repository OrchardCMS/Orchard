using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.ContentManagement.Handlers {
    public abstract class TemplateFilterBase<TPart> : IContentTemplateFilter where TPart : class, IContent {

        protected virtual void GetItemMetadata(GetItemMetadataContext context, TPart instance) { }
        protected virtual void BuildDisplayModel(BuildDisplayModelContext context, TPart instance) { }
        protected virtual void BuildEditorModel(BuildEditorModelContext context, TPart instance) { }
        protected virtual void UpdateEditorModel(UpdateEditorModelContext context, TPart instance) { }

        void IContentTemplateFilter.GetItemMetadata(GetItemMetadataContext context) {
            if (context.ContentItem.Is<TPart>())
                GetItemMetadata(context, context.ContentItem.As<TPart>());
        }

        void IContentTemplateFilter.BuildDisplayModel(BuildDisplayModelContext context) {
            if (context.ContentItem.Is<TPart>())
                BuildDisplayModel(context, context.ContentItem.As<TPart>());
        }

        void IContentTemplateFilter.BuildEditorModel(BuildEditorModelContext context) {
            if (context.ContentItem.Is<TPart>())
                BuildEditorModel(context, context.ContentItem.As<TPart>());
        }

        void IContentTemplateFilter.UpdateEditorModel(UpdateEditorModelContext context) {
            if (context.ContentItem.Is<TPart>())
                UpdateEditorModel(context, context.ContentItem.As<TPart>());
        }

    }
}
