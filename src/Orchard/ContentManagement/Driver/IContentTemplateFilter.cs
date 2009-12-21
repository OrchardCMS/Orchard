using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.ContentManagement.Handlers {
    interface IContentTemplateFilter : IContentFilter {
        void GetItemMetadata(GetItemMetadataContext context);
        void BuildDisplayModel(BuildDisplayModelContext context);
        void BuildEditorModel(BuildEditorModelContext context);
        void UpdateEditorModel(UpdateEditorModelContext context);
    }
}
