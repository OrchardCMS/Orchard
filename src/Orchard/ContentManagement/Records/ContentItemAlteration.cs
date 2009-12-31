using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using JetBrains.Annotations;

namespace Orchard.ContentManagement.Records {
    class ContentItemAlteration : IAutoMappingAlteration {
        private readonly IEnumerable<Type> _recordTypes;

        [UsedImplicitly]
        public ContentItemAlteration() {
            _recordTypes = Enumerable.Empty<Type>();
        }
        public ContentItemAlteration(IEnumerable<Type> recordTypes) {
            _recordTypes = recordTypes;
        }

        public void Alter(AutoPersistenceModel model) {

            model.Override<ContentItemRecord>(mapping => {
                foreach (var recordType in _recordTypes.Where(Utility.IsPartRecord)) {
                    var type = typeof(Alteration<,>).MakeGenericType(typeof(ContentItemRecord), recordType);
                    var alteration = (IAlteration<ContentItemRecord>)Activator.CreateInstance(type);
                    alteration.Override(mapping);
                }
            });

            model.Override<ContentItemVersionRecord>(mapping => {
                foreach (var recordType in _recordTypes.Where(Utility.IsPartVersionRecord)) {
                    var type = typeof(Alteration<,>).MakeGenericType(typeof(ContentItemVersionRecord), recordType);
                    var alteration = (IAlteration<ContentItemVersionRecord>)Activator.CreateInstance(type);
                    alteration.Override(mapping);
                }
            });
        }

        interface IAlteration<TItemRecord> {
            void Override(AutoMapping<TItemRecord> mapping);
        }

        class Alteration<TItemRecord, TPartRecord> : IAlteration<TItemRecord> {
            public void Override(AutoMapping<TItemRecord> mapping) {

                // public TPartRecord TPartRecord {get;set;}
                var name = typeof(TPartRecord).Name;
                var syntheticMethod = new DynamicMethod(name, typeof(TPartRecord), null, typeof(TItemRecord));
                var syntheticProperty = new SyntheticPropertyInfo(syntheticMethod);

                // record => record.TPartRecord
                var parameter = Expression.Parameter(typeof(TItemRecord), "record");
                var syntheticExpression = (Expression<Func<TItemRecord, TPartRecord>>)Expression.Lambda(
                    typeof(Func<TItemRecord, TPartRecord>),
                    Expression.Property(parameter, syntheticProperty),
                    parameter);

                mapping.References(syntheticExpression)
                    .Access.NoOp()
                    .Column("Id")
                    .Unique()
                    .Not.Insert()
                    .Not.Update()
                    .Cascade.All();
            }
        }
        class PartAlteration<TPartRecord> : IAlteration<ContentItemRecord> where TPartRecord : ContentPartRecord {
            public void Override(AutoMapping<ContentItemRecord> mapping) {

                // public TPartRecord TPartRecord {get;set;}
                var name = typeof(TPartRecord).Name;
                var syntheticMethod = new DynamicMethod(name, typeof(TPartRecord), null, typeof(ContentItemRecord));
                var syntheticProperty = new SyntheticPropertyInfo(syntheticMethod);

                // record => record.TPartRecord
                var parameter = Expression.Parameter(typeof(ContentItemRecord), "record");
                var syntheticExpression = (Expression<Func<ContentItemRecord, TPartRecord>>)Expression.Lambda(
                    typeof(Func<ContentItemRecord, TPartRecord>),
                    Expression.Property(parameter, syntheticProperty),
                    parameter);

                mapping.References(syntheticExpression)
                    .Access.NoOp()
                    .Column("Id")
                    .Unique()
                    .Not.Insert()
                    .Not.Update()
                    .Cascade.All();
            }
        }

        class PartVersionAlteration<TPartRecord> : IAlteration<ContentItemVersionRecord> where TPartRecord : ContentPartVersionRecord {
            public void Override(AutoMapping<ContentItemVersionRecord> mapping) {

                // public TPartRecord TPartRecord {get;set;}
                var name = typeof(TPartRecord).Name;
                var syntheticMethod = new DynamicMethod(name, typeof(TPartRecord), null, typeof(ContentItemVersionRecord));
                var syntheticProperty = new SyntheticPropertyInfo(syntheticMethod);

                // record => record.TPartRecord
                var parameter = Expression.Parameter(typeof(ContentItemVersionRecord), "record");
                var syntheticExpression = (Expression<Func<ContentItemVersionRecord, TPartRecord>>)Expression.Lambda(
                    typeof(Func<ContentItemVersionRecord, TPartRecord>),
                    Expression.Property(parameter, syntheticProperty),
                    parameter);

                mapping.References(syntheticExpression)
                    .Access.NoOp()
                    .Column("Id")
                    .Unique()
                    .Not.Insert()
                    .Not.Update()
                    .Cascade.All();
            }
        }
        private class SyntheticPropertyInfo : PropertyInfo {
            private readonly DynamicMethod _getMethod;

            public SyntheticPropertyInfo(DynamicMethod dynamicMethod) {
                _getMethod = dynamicMethod;
            }

            public override object[] GetCustomAttributes(bool inherit) {
                throw new NotImplementedException();
            }

            public override bool IsDefined(Type attributeType, bool inherit) {
                throw new NotImplementedException();
            }

            public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) {
                throw new NotImplementedException();
            }

            public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) {
                throw new NotImplementedException();
            }

            public override MethodInfo[] GetAccessors(bool nonPublic) {
                throw new NotImplementedException();
            }

            public override MethodInfo GetGetMethod(bool nonPublic) {
                return _getMethod;
            }

            public override MethodInfo GetSetMethod(bool nonPublic) {
                throw new NotImplementedException();
            }

            public override ParameterInfo[] GetIndexParameters() {
                throw new NotImplementedException();
            }

            public override string Name {
                get { return _getMethod.Name; }
            }

            public override Type DeclaringType {
                get { throw new NotImplementedException(); }
            }

            public override Type ReflectedType {
                get { throw new NotImplementedException(); }
            }

            public override Type PropertyType {
                get { return _getMethod.ReturnType; }
            }

            public override PropertyAttributes Attributes {
                get { throw new NotImplementedException(); }
            }

            public override bool CanRead {
                get { return true; }
            }

            public override bool CanWrite {
                get { throw new NotImplementedException(); }
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
                throw new NotImplementedException();
            }
        }
    }
}