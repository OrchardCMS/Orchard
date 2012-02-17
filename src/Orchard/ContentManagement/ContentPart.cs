using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using ClaySharp;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.Utilities;
using Orchard.UI;

namespace Orchard.ContentManagement {
    public class ContentPart : IContent, IContentBehavior, IDynamicMetaObjectProvider {
        private readonly IList<ContentField> _fields;

        public ContentPart() {
            _behavior = new ClayBehaviorCollection(new[] { new ContentPartBehavior(this) });
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

        private readonly IClayBehavior _behavior;
        IClayBehavior IContentBehavior.Behavior {
            get { return _behavior; }
        }
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) {
            return new ClayMetaObject(this, parameter, ex => Expression.Property(Expression.Convert(ex, typeof(IContentBehavior)), "Behavior"));
        }

        /// <summary>
        /// The ContentItem's identifier.
        /// </summary>
        [HiddenInput(DisplayValue = false)]
        public int Id {
            get { return ContentItem.Id; }
        }

        public ContentTypeDefinition TypeDefinition { get { return ContentItem.TypeDefinition; } }
        public ContentTypePartDefinition TypePartDefinition { get; set; }
        public ContentPartDefinition PartDefinition { get { return TypePartDefinition.PartDefinition; } }
        public SettingsDictionary Settings { get { return TypePartDefinition.Settings; } }

        public IEnumerable<ContentField> Fields { get { return _fields; } }


        public bool Has(Type fieldType, string fieldName) {
            return _fields.Any(field => fieldType.IsInstanceOfType(field) && field.Name == fieldName);
        }

        public ContentField Get(Type fieldType, string fieldName) {
            return _fields.FirstOrDefault(field => fieldType.IsInstanceOfType(field) && field.Name == fieldName);
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
