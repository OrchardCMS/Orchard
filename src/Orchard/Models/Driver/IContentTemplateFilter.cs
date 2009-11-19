using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.Models.Driver {
    interface IContentTemplateFilter : IContentFilter {
        void GetEditors(GetContentEditorsContext context);
        void UpdateEditors(UpdateContentContext context);
    }
}
