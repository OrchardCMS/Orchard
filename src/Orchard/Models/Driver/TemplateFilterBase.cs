using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.Models.Driver {
    public abstract class TemplateFilterBase<TPart> : IContentTemplateFilter where TPart : class, IContent {

        protected virtual void GetItemMetadata(GetItemMetadataContext context, TPart instance) { }
        protected virtual void GetDisplays(GetDisplaysContext context, TPart instance) { }
        protected virtual void GetEditors(GetEditorsContext context, TPart instance) { }
        protected virtual void UpdateEditors(UpdateContentContext context, TPart instance) { }

        void IContentTemplateFilter.GetItemMetadata(GetItemMetadataContext context) {
            if (context.ContentItem.Is<TPart>())
                GetItemMetadata(context, context.ContentItem.As<TPart>());
        }

        void IContentTemplateFilter.GetDisplays(GetDisplaysContext context) {
            if (context.ContentItem.Is<TPart>())
                GetDisplays(context, context.ContentItem.As<TPart>());
        }

        void IContentTemplateFilter.GetEditors(GetEditorsContext context) {
            if (context.ContentItem.Is<TPart>())
                GetEditors(context, context.ContentItem.As<TPart>());
        }

        void IContentTemplateFilter.UpdateEditors(UpdateContentContext context) {
            if (context.ContentItem.Is<TPart>())
                UpdateEditors(context, context.ContentItem.As<TPart>());
        }

    }
}
