using System.Collections.Generic;
using System.Reflection;
using Orchard.Events;
using Orchard.Localization;

namespace Orchard.Projections.Services {
    public interface IMemberBindingProvider : IEventHandler {
        void GetMemberBindings(BindingBuilder builder);
    }

    public class BindingBuilder {
        private readonly IList<BindingItem> _memberBindings;
        public BindingBuilder() {
            _memberBindings = new List<BindingItem>();
        }

        public BindingBuilder Add(PropertyInfo property, string display, string description) {
            return Add(property, new LocalizedString(display), new LocalizedString(description));
        }

        public BindingBuilder Add(PropertyInfo property, LocalizedString display, LocalizedString description) {
            _memberBindings.Add( new BindingItem{
                Property = property,
                DisplayName = display,
                Description = description
            });
            return this;
        }

        public IEnumerable<BindingItem> Build() {
            return _memberBindings;
        }
    }

    public class BindingItem {
        public virtual PropertyInfo Property { get; set; }
        public virtual LocalizedString Description { get; set; }
        public virtual LocalizedString DisplayName { get; set; }
    }
}