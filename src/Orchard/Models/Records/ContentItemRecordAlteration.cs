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

namespace Orchard.Models.Records {
    class ContentItemRecordAlteration : IAutoMappingAlteration {
        private readonly IEnumerable<Type> _recordTypes;

        public ContentItemRecordAlteration() {
            _recordTypes = Enumerable.Empty<Type>();
        }
        public ContentItemRecordAlteration(IEnumerable<Type> recordTypes) {
            _recordTypes = recordTypes;
        }

        public void Alter(AutoPersistenceModel model) {
            model.Override<ContentItemRecord>(mapping => {
                foreach (var recordType in _recordTypes.Where(x => x.IsSubclassOf(typeof(ContentPartRecord)))) {
                    var type = typeof(Alteration<>).MakeGenericType(recordType);
                    var alteration = (IAlteration)Activator.CreateInstance(type);
                    alteration.Override(mapping);
                }
            });
        }

        interface IAlteration {
            void Override(AutoMapping<ContentItemRecord> mapping);
        }

        class Alteration<TPartRecord> : IAlteration where TPartRecord : ContentPartRecord {
            public void Override(AutoMapping<ContentItemRecord> mapping) {
                var name = typeof(TPartRecord).Name;


                var syntheticMethod = new DynamicMethod(name, typeof(TPartRecord), null, typeof(ContentItemRecord));
                var syntheticProperty = new FakePropertyInfo(syntheticMethod);

                var parameter = Expression.Parameter(typeof(ContentItemRecord), "record");
                var syntheticExpression = (Expression<Func<ContentItemRecord, TPartRecord>>)Expression.Lambda(
                    typeof(Func<ContentItemRecord, TPartRecord>),
                    Expression.Property(parameter, syntheticProperty),
                    parameter);

                mapping.HasOne(syntheticExpression).Access.NoOp().Fetch.Select();
            }

            private class FakePropertyInfo : PropertyInfo {
                private readonly DynamicMethod _getMethod;

                public FakePropertyInfo(DynamicMethod dynamicMethod) {
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
}