using System;
using System.Collections.Generic;
using Orchard.Localization;

namespace Orchard.Workflows.Models.Descriptors {
    public class DescribeActivityFor {
        private readonly string _category;

        public DescribeActivityFor(string category, LocalizedString name, LocalizedString description) {
            Types = new List<ActivityDescriptor>();
            _category = category;
            Name = name;
            Description = description;
        }

        public LocalizedString Name { get; private set; }
        public LocalizedString Description { get; private set; }
        public List<ActivityDescriptor> Types { get; private set; }

        public DescribeActivityFor Element(string type, LocalizedString name, LocalizedString description, Func<ActivityContext, bool> action, Func<ActivityContext, LocalizedString> display, string form = null) {
            Types.Add(new ActivityDescriptor { Type = type, Name = name, Description = description, Category = _category, Action = action, Display = display, Form = form });
            return this;
        }
    }
}