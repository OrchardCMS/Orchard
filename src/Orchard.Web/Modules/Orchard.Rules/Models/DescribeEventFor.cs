using System;
using System.Collections.Generic;
using Orchard.Localization;

namespace Orchard.Rules.Models {
    public class DescribeEventFor {
        private readonly string _category;

        public DescribeEventFor(string category, LocalizedString name, LocalizedString description) {
            Types = new List<EventDescriptor>();
            _category = category;
            Name = name;
            Description = description;
        }

        public LocalizedString Name { get; private set; }
        public LocalizedString Description { get; private set; }
        public List<EventDescriptor> Types { get; private set; }

        public DescribeEventFor Element(string type, LocalizedString name, LocalizedString description, Func<EventContext, bool> condition, Func<EventContext, LocalizedString> display, string form = null) {
            Types.Add(new EventDescriptor { Type = type, Name = name, Description = description, Category = _category, Condition = condition, Display = display, Form = form });
            return this;
        }
    }
}