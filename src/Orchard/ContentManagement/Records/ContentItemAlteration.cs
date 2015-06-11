using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Mapping;
using Orchard.Environment.ShellBuilders.Models;

namespace Orchard.ContentManagement.Records {
    class ContentItemAlteration : IAutoMappingAlteration {
        private readonly IEnumerable<RecordBlueprint> _recordDescriptors;

        public ContentItemAlteration() {
            _recordDescriptors = Enumerable.Empty<RecordBlueprint>();
        }

        public ContentItemAlteration(IEnumerable<RecordBlueprint> recordDescriptors) {
            _recordDescriptors = recordDescriptors;
        }

        public void Alter(AutoPersistenceModel model) {

            model.Override<ContentItemRecord>(mapping => {
                foreach (var descriptor in _recordDescriptors.Where(d => Utility.IsPartRecord(d.Type))) {
                    var type = typeof(Alteration<,>).MakeGenericType(typeof(ContentItemRecord), descriptor.Type);
                    var alteration = (IAlteration<ContentItemRecord>)Activator.CreateInstance(type);
                    alteration.Override(mapping);
                }
                mapping.IgnoreProperty(x => x.Infoset);
            });

            model.Override<ContentItemVersionRecord>(mapping => {
                foreach (var descriptor in _recordDescriptors.Where(d => Utility.IsPartVersionRecord(d.Type))) {
                    var type = typeof(Alteration<,>).MakeGenericType(typeof(ContentItemVersionRecord), descriptor.Type);
                    var alteration = (IAlteration<ContentItemVersionRecord>)Activator.CreateInstance(type);
                    alteration.Override(mapping);
                }
                mapping.IgnoreProperty(x => x.Infoset);
            });
        }

        interface IAlteration<TItemRecord> {
            void Override(AutoMapping<TItemRecord> mapping);
        }

        /// <summary>
        /// Add a "fake" column to the automapping record so that the column can be
        /// referenced when building joins accross content item record tables.
        /// <typeparam name="TItemRecord">Either ContentItemRecord or ContentItemVersionRecord</typeparam>
        /// <typeparam name="TPartRecord">A part record (deriving from TItemRecord)</typeparam>
        /// </summary>
        class Alteration<TItemRecord, TPartRecord> : IAlteration<TItemRecord> {
            public void Override(AutoMapping<TItemRecord> mapping) {

                // public TPartRecord TPartRecord {get;set;}
                var name = typeof(TPartRecord).Name;
                var dynamicMethod = new DynamicMethod(name, typeof(TPartRecord), null, typeof(TItemRecord));
                var syntheticMethod = new SyntheticMethodInfo(dynamicMethod, typeof(TItemRecord));
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

        /// <summary>
        /// Synthetic method around a dynamic method. We need this so that we can
        /// override the "static" method attributes, and also return a valid "DeclaringType".
        /// </summary>
        public class SyntheticMethodInfo : MethodInfo {
            private readonly DynamicMethod _dynamicMethod;
            private readonly Type _declaringType;

            public SyntheticMethodInfo(DynamicMethod dynamicMethod, Type declaringType) {
                _dynamicMethod = dynamicMethod;
                _declaringType = declaringType;
            }

            public override object[] GetCustomAttributes(bool inherit) {
                return _dynamicMethod.GetCustomAttributes(inherit);
            }

            public override bool IsDefined(Type attributeType, bool inherit) {
                return IsDefined(attributeType, inherit);
            }

            public override ParameterInfo[] GetParameters() {
                return _dynamicMethod.GetParameters();
            }

            public override MethodImplAttributes GetMethodImplementationFlags() {
                return _dynamicMethod.GetMethodImplementationFlags();
            }

            public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture) {
                return _dynamicMethod.Invoke(obj, invokeAttr, binder, parameters, culture);
            }

            public override MethodInfo GetBaseDefinition() {
                return _dynamicMethod.GetBaseDefinition();
            }

            public override ICustomAttributeProvider ReturnTypeCustomAttributes {
                get { return ReturnTypeCustomAttributes; }
            }

            public override string Name {
                get { return _dynamicMethod.Name; }
            }

            public override Type DeclaringType {
                get { return _declaringType; }
            }

            public override Type ReflectedType {
                get { return _dynamicMethod.ReflectedType; }
            }

            public override RuntimeMethodHandle MethodHandle {
                get { return _dynamicMethod.MethodHandle; }
            }

            public override MethodAttributes Attributes {
                get { return _dynamicMethod.Attributes & ~MethodAttributes.Static; }
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
                return _dynamicMethod.GetCustomAttributes(attributeType, inherit);
            }

            public override Type ReturnType {
                get { return _dynamicMethod.ReturnType; }
            }
        }

        /// <summary>
        /// Synthetic property around a method info (the "getter" method).
        /// This is a minimal implementation enabling support for AutoMapping.References.
        /// </summary>
        public class SyntheticPropertyInfo : PropertyInfo {
            private readonly MethodInfo _getMethod;

            public SyntheticPropertyInfo(MethodInfo getMethod) {
                _getMethod = getMethod;
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
                return null;
            }

            public override ParameterInfo[] GetIndexParameters() {
                throw new NotImplementedException();
            }

            public override string Name {
                get { return _getMethod.Name; }
            }

            public override Type DeclaringType {
                get { return _getMethod.DeclaringType; }
            }

            public override Type ReflectedType {
                get { return _getMethod.ReflectedType; }
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
                return _getMethod.GetCustomAttributes(attributeType, inherit);
            }

            public override int MetadataToken {
                get { return 0; }
            }

            public override Module Module {
                get { return null; }
            }

            public override MemberTypes MemberType {
                get { return MemberTypes.Property; }
            }
        }
    }
}
