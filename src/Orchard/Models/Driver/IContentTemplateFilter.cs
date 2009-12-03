using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.Models.Driver {
    interface IContentTemplateFilter : IContentFilter {
        void GetItemMetadata(GetItemMetadataContext context);
        void GetDisplayViewModel(GetDisplayViewModelContext context);
        void GetEditorViewModel(GetEditorViewModelContext context);
        void UpdateEditorViewModel(UpdateEditorViewModelContext context);
    }
}
