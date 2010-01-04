using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.UI.Zones;

namespace Orchard.ContentManagement.ViewModels {
    public class ItemDisplayModel : IZoneContainer {
        private ContentItem _item;

        protected ItemDisplayModel() {
            Zones = new ZoneCollection();
        }

        protected ItemDisplayModel(ItemDisplayModel displayModel) {
            TemplateName = displayModel.TemplateName;
            Prefix = displayModel.Prefix;
            Zones = displayModel.Zones;
            Item = displayModel.Item;
        }

        public ItemDisplayModel(ContentItem item) {
            Item = item;
            Zones = new ZoneCollection();
        }

        public ContentItem Item {
            get { return _item; }
            set { SetItem(value); }
        }

        protected virtual void SetItem(ContentItem value) {
            _item = value;
        }

        public Func<HtmlHelper, ItemDisplayModel, HtmlHelper> Adaptor { get; set; }
        public string TemplateName { get; set; }
        public string Prefix { get; set; }
        public ZoneCollection Zones { get; set; }
        
            
        public IEnumerable<TemplateViewModel> Displays {
            get {
                return Zones
                    .SelectMany(z => z.Value.Items
                        .OfType<PartDisplayZoneItem>()
                        .Select(x=>new{ZoneName=z.Key,Item=x}))                    
                    .Select(x => new TemplateViewModel(x.Item.Model,x.Item.Prefix) {
                        Model = x.Item.Model,
                        TemplateName=x.Item.TemplateName,
                        WasUsed=x.Item.WasExecuted,
                        ZoneName=x.ZoneName,
                    });
            }
        }
        
    }

    public class ItemDisplayModel<TPart> : ItemDisplayModel where TPart : IContent {
        private TPart _item;

        public ItemDisplayModel() {

        }

        public ItemDisplayModel(ItemDisplayModel displayModel)
            : base(displayModel) {
        }

        public new TPart Item {
            get { return _item; }
            set { SetItem(value.ContentItem); }
        }

        protected override void SetItem(ContentItem value) {
            _item = value.As<TPart>();
            base.SetItem(value);
        }
    }
}