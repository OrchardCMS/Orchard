using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Property;
using Orchard.Projections.PropertyEditors;
using Orchard.Projections.Services;
using Orchard.Utility.Extensions;

namespace Orchard.Projections.Providers.Properties {
    public class MemberBindingProperties : IPropertyProvider {
        private readonly IEnumerable<IMemberBindingProvider> _bindingProviders;
        private readonly IPropertyFormater _propertyFormater;

        public MemberBindingProperties(
            IEnumerable<IMemberBindingProvider> bindingProviders,
            IPropertyFormater propertyFormater
            ) {
            _bindingProviders = bindingProviders;
            _propertyFormater = propertyFormater;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribePropertyContext describe) {
            var builder = new BindingBuilder();

            foreach(var bindingProvider in _bindingProviders) {
                bindingProvider.GetMemberBindings(builder);
            }

            var groupedMembers = builder.Build().GroupBy(b => b.Property.DeclaringType).ToDictionary(b => b.Key, b => b);

            foreach (var typeMembers in groupedMembers.Keys) {
                var descriptor = describe.For(typeMembers.Name, new LocalizedString(typeMembers.Name.CamelFriendly()), T("Members for {0}", typeMembers.Name));
                foreach(var member in groupedMembers[typeMembers]) {
                    var closureMember = member;
                    descriptor.Element(member.Property.Name, member.DisplayName, member.Description,
                        context => Display(context, closureMember.Property),
                        (context, contentItem) => Render(context, closureMember.Property, contentItem),
                        _propertyFormater.GetForm(member.Property.PropertyType)
                    );
                }
            }
        }

        public LocalizedString Display(PropertyContext context, PropertyInfo property) {
            return T("{0} {1}", property.DeclaringType.Name, property.Name);
        }

        public dynamic Render(PropertyContext context, PropertyInfo property, ContentItem contentItem) {
            // creating type for ContentPart<TRecord>, where TRecord is the type of the property
            var partRecordType = typeof (ContentPart<>).MakeGenericType(new [] {property.DeclaringType});

            // get the part for this ContentPart<TRecord>
            var part = contentItem.Parts.FirstOrDefault(p => partRecordType.IsAssignableFrom(p.GetType()));
            var record = partRecordType.GetProperty("Record").GetValue(part, null);
            var value = property.GetValue(record, null);

            if(value == null) {
                return null;
            }

            // call specific formatter rendering
            return _propertyFormater.Format(property.PropertyType, value, context.State);
        }
    }
}