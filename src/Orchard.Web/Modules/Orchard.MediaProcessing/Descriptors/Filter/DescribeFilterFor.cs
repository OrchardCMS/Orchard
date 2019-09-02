using System;
using System.Collections.Generic;
using Orchard.Localization;

namespace Orchard.MediaProcessing.Descriptors.Filter {
    public class DescribeFilterFor {
        private readonly string _category;

        public DescribeFilterFor(string category, LocalizedString name, LocalizedString description) {
            Types = new List<FilterDescriptor>();
            _category = category;
            Name = name;
            Description = description;
        }

        public LocalizedString Name { get; private set; }
        public LocalizedString Description { get; private set; }
        public List<FilterDescriptor> Types { get; private set; }

        public DescribeFilterFor Element(string type, LocalizedString name, LocalizedString description, Action<FilterContext> filter, Func<FilterContext, LocalizedString> display, string form = null) {
            Types.Add(new FilterDescriptor { Type = type, Name = name, Description = description, Category = _category, Filter = filter, Display = display, Form = form });
            return this;
        }
    }
}