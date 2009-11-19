using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.Models.Driver {
    interface IContentTemplateFilter : IContentFilter {
        void GetDisplays(GetDisplaysContext context);
        void GetEditors(GetEditorsContext context);
        void UpdateEditors(UpdateContentContext context);
    }
}
