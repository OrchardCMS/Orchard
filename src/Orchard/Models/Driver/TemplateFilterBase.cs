using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.Models.Driver {
    public abstract class TemplateFilterBase<TPart> : IContentTemplateFilter where TPart : class, IContentItemPart {

        protected virtual void GetEditors(GetContentEditorsContext context, TPart instance) { }
        protected virtual void UpdateEditors(UpdateContentContext context, TPart instance) { }

        void IContentTemplateFilter.GetEditors(GetContentEditorsContext context) {
            if (context.ContentItem.Is<TPart>())
                GetEditors(context, context.ContentItem.As<TPart>());
        }

        void IContentTemplateFilter.UpdateEditors(UpdateContentContext context) {
            if (context.ContentItem.Is<TPart>())
                UpdateEditors(context, context.ContentItem.As<TPart>());
        }

    }
}
