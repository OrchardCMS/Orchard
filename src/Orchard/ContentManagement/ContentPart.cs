using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Autofac;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement.Utilities;
using Orchard.UI;

namespace Orchard.ContentManagement {
    public class ContentPart : DynamicObject, IContent {
        private readonly IList<ContentField> _fields;

        public ContentPart() {
            _fields = new List<ContentField>();
        }


        public virtual ContentItem ContentItem { get; set; }

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
            return _fields.Any(field => field.Name == fieldName && fieldType.IsInstanceOfType(field));
        }

        public ContentField Get(Type fieldType, string fieldName) {
            return _fields.FirstOrDefault(field => field.Name == fieldName && fieldType.IsInstanceOfType(field));
        }

        public void Weld(ContentField field) {
            _fields.Add(field);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) {

            var found = base.TryGetMember(binder, out result);
            if (!found) {
                foreach (var part in ContentItem.Parts) {
                    if (part.PartDefinition.Name == binder.Name) {
                        result = part;
                        return true;
                    }
                }

                foreach (var field in Fields) {
                    if (field.PartFieldDefinition.Name == binder.Name) {
                        result = field;
                        return true;
                    }
                }
                result = null;
                return true;
            }

            return true;
        }

        public T Retrieve<T>(string fieldName) {
            return InfosetHelper.Retrieve<T>(this, fieldName);
        }

        public T RetrieveVersioned<T>(string fieldName) {
            return this.Retrieve<T>(fieldName, true);
        }

        public virtual void Store<T>(string fieldName, T value) {
            InfosetHelper.Store(this, fieldName, value);
        }

        public virtual void StoreVersioned<T>(string fieldName, T value) {
            this.Store(fieldName, value, true);
        }

    }

    public class ContentPart<TRecord> : ContentPart {

        static protected bool IsVersionableRecord { get; private set;}

        static ContentPart() {
            IsVersionableRecord = typeof (TRecord).IsAssignableTo<ContentItemVersionRecord>();
        }

        protected TProperty Retrieve<TProperty>(Expression<Func<TRecord, TProperty>> targetExpression) {
            return InfosetHelper.Retrieve(this, targetExpression);
        }

        protected TProperty Retrieve<TProperty>(
            Expression<Func<TRecord, TProperty>> targetExpression,
            Func<TRecord, TProperty> defaultExpression) {

            return InfosetHelper.Retrieve(this, targetExpression, defaultExpression);
        }
        protected TProperty Retrieve<TProperty>(
                    Expression<Func<TRecord, TProperty>> targetExpression,
                    TProperty defaultValue) {

            return InfosetHelper.Retrieve(this, targetExpression, (Func<TRecord, TProperty>)(x => defaultValue));
        }

        protected ContentPart<TRecord> Store<TProperty>(
            Expression<Func<TRecord, TProperty>> targetExpression,
            TProperty value) {

            InfosetHelper.Store(this, targetExpression, value);
            return this;
        }

        public readonly LazyField<TRecord> _record = new LazyField<TRecord>();
        public TRecord Record { get { return _record.Value; } set { _record.Value = value; } }
    }

}
