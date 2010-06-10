using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;

namespace Orchard.UI.Navigation {
    public class NavigationBuilder {
        IEnumerable<MenuItem> Contained { get; set; }

        public NavigationBuilder Add(LocalizedString caption, string position, Action<NavigationItemBuilder> itemBuilder) {
            var childBuilder = new NavigationItemBuilder();

            childBuilder.Caption(caption);
            childBuilder.Position(position);
            itemBuilder(childBuilder);
            Contained = (Contained ?? Enumerable.Empty<MenuItem>()).Concat(childBuilder.Build());
            return this;
        }

        public NavigationBuilder Add(LocalizedString caption, Action<NavigationItemBuilder> itemBuilder) {
            return Add(caption, null, itemBuilder);
        }
        public NavigationBuilder Add(Action<NavigationItemBuilder> itemBuilder) {
            return Add(null, null, itemBuilder);
        }
        public NavigationBuilder Add(LocalizedString caption, string position) {
            return Add(caption, position, x=> { });
        }
        public NavigationBuilder Add(LocalizedString caption) {
            return Add(caption, null, x => { });
        }

        public IEnumerable<MenuItem> Build() {
            return (Contained ?? Enumerable.Empty<MenuItem>()).ToList();
        }
    }
}