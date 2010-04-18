using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using JetBrains.Annotations;
using Orchard.Environment;

namespace Orchard.ContentManagement.Records {
    class ContentItemAlteration : IAutoMappingAlteration {
        private readonly IEnumerable<RecordDescriptor_Obsolete> _recordDescriptors;

        [UsedImplicitly]
        public ContentItemAlteration() {
            _recordDescriptors = Enumerable.Empty<RecordDescriptor_Obsolete>();
        }

        public ContentItemAlteration(IEnumerable<RecordDescriptor_Obsolete> recordDescriptors) {
            _recordDescriptors = recordDescriptors;
        }

        public void Alter(AutoPersistenceModel model) {

            model.Override<ContentItemRecord>(mapping => {
                foreach (var descriptor in _recordDescriptors.Where(d => Utility.IsPartRecord(d.Type))) {
                    var type = typeof(Alteration<,>).MakeGenericType(typeof(ContentItemRecord), descriptor.Type);
                    var alteration = (IAlteration<ContentItemRecord>)Activator.CreateInstance(type);
                    alteration.Override(mapping);
                }
            });

            model.Override<ContentItemVersionRecord>(mapping => {
                foreach (var descriptor in _recordDescriptors.Where(d => Utility.IsPartVersionRecord(d.Type))) {
                    var type = typeof(Alteration<,>).MakeGenericType(typeof(ContentItemVersionRecord), descriptor.Type);
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
                    .ForeignKey("none") // prevent foreign key constraint from ContentItem(Version)Record to TPartRecord
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