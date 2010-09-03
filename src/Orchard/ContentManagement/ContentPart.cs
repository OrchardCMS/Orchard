using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.Utilities;
using Orchard.UI;

namespace Orchard.ContentManagement {
    public class ContentPart : IContent {
        private readonly IList<ContentField> _fields;

        public ContentPart() {
            _fields = new List<ContentField>();
        }

        public virtual ContentItem ContentItem { get; set; }

        //interesting thought, should/could parts also have zones (would then have zones on the page, content item and parts...)?
        private readonly IZoneCollection _zones = new ZoneCollection();
        public virtual IZoneCollection Zones {
            get {
                return _zones;
            }
        }

        public ContentTypeDefinition TypeDefinition { get { return ContentItem.TypeDefinition; } }
        public ContentTypePartDefinition TypePartDefinition { get; set; }
        public ContentPartDefinition PartDefinition { get { return TypePartDefinition.PartDefinition; } }
        public SettingsDictionary Settings { get { return TypePartDefinition.Settings; } }

        public IEnumerable<ContentField> Fields { get { return _fields; } }


        public bool Has(Type fieldType, string fieldName) {
            return _fields.Any(field => fieldType.IsAssignableFrom(field.GetType()) && field.Name == fieldName);
        }

        public ContentField Get(Type fieldType, string fieldName) {
            return _fields.FirstOrDefault(field => fieldType.IsAssignableFrom(field.GetType()) && field.Name == fieldName);
        }

        public void Weld(ContentField field) {
            _fields.Add(field);
        }
    }

    public class ContentPart<TRecord> : ContentPart {
        public readonly LazyField<TRecord> _record = new LazyField<TRecord>();
        public TRecord Record { get { return _record.Value; } set { _record.Value = value; } }
    }

}
