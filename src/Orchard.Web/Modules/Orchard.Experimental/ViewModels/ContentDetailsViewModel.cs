using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Experimental.ViewModels {

    public class ContentDetailsViewModel  {
        public IContent Item { get; set; }

        public IEnumerable<Type> PartTypes { get; set; }

        public IContent DisplayShape { get; set; }

        public IContent EditorShape { get; set; }

        public IEnumerable<TemplateViewModel> Editors {
            get {
#if REFACTORING                
                return EditorModel.Zones
                    .SelectMany(z => z.Value.Items
                        .OfType<ContentPartEditorZoneItem>()
                        .Select(x => new { ZoneName = z.Key, Item = x }))
                    .Select(x => new TemplateViewModel(x.Item.Model, x.Item.Prefix) {
                        Model = x.Item.Model,
                        TemplateName = x.Item.TemplateName,
                        WasUsed = x.Item.WasExecuted,
                        ZoneName = x.ZoneName,
                    });
#else
                return null;
#endif
            }
        }

        public object Locate(Type type) {
            return Item.ContentItem.Get(type);
        }
    }
}
