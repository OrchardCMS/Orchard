using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement.ViewModels;
using Orchard.Mvc.ViewModels;
using Orchard.UI.Zones;

namespace Orchard.DevTools.ViewModels {
    public class ContentDetailsViewModel : BaseViewModel {
        public IContent Item { get; set; }

        public IEnumerable<Type> PartTypes { get; set; }

        public ContentItemViewModel DisplayModel { get; set; }

        public ContentItemViewModel EditorModel { get; set; }

        public IEnumerable<TemplateViewModel> Displays {
            get {
                return DisplayModel.Zones
                    .SelectMany(z => z.Value.Items
                        .OfType<ContentPartDisplayZoneItem>()
                        .Select(x=>new{ZoneName=z.Key,Item=x}))                    
                    .Select(x => new TemplateViewModel(x.Item.Model,x.Item.Prefix) {
                        Model = x.Item.Model,
                        TemplateName=x.Item.TemplateName,
                        WasUsed=x.Item.WasExecuted,
                        ZoneName=x.ZoneName,
                    });
            }
        }

        public IEnumerable<TemplateViewModel> Editors {
            get {
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
            }
        }

        public object Locate(Type type) {
            return Item.ContentItem.Get(type);
        }
    }
}
