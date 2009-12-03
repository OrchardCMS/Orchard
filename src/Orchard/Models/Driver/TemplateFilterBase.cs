using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.Models.Driver {
    public abstract class TemplateFilterBase<TPart> : IContentTemplateFilter where TPart : class, IContent {

        protected virtual void GetItemMetadata(GetItemMetadataContext context, TPart instance) { }
        protected virtual void GetDisplayViewModel(GetDisplayViewModelContext context, TPart instance) { }
        protected virtual void GetEditorViewModel(GetEditorViewModelContext context, TPart instance) { }
        protected virtual void UpdateEditorViewModel(UpdateEditorViewModelContext context, TPart instance) { }

        void IContentTemplateFilter.GetItemMetadata(GetItemMetadataContext context) {
            if (context.ContentItem.Is<TPart>())
                GetItemMetadata(context, context.ContentItem.As<TPart>());
        }

        void IContentTemplateFilter.GetDisplayViewModel(GetDisplayViewModelContext context) {
            if (context.ContentItem.Is<TPart>())
                GetDisplayViewModel(context, context.ContentItem.As<TPart>());
        }

        void IContentTemplateFilter.GetEditorViewModel(GetEditorViewModelContext context) {
            if (context.ContentItem.Is<TPart>())
                GetEditorViewModel(context, context.ContentItem.As<TPart>());
        }

        void IContentTemplateFilter.UpdateEditorViewModel(UpdateEditorViewModelContext context) {
            if (context.ContentItem.Is<TPart>())
                UpdateEditorViewModel(context, context.ContentItem.As<TPart>());
        }

    }
}
