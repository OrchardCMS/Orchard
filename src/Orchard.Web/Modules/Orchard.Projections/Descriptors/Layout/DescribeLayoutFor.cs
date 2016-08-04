using System;
using System.Collections.Generic;
using Orchard.Localization;

namespace Orchard.Projections.Descriptors.Layout {
    public class DescribeLayoutFor {
        private readonly string _category;

        public DescribeLayoutFor(string category, LocalizedString name, LocalizedString description) {
            Types = new List<LayoutDescriptor>();
            _category = category;
            Name = name;
            Description = description;
        }

        public LocalizedString Name { get; private set; }
        public LocalizedString Description { get; private set; }
        public List<LayoutDescriptor> Types { get; private set; }

        public DescribeLayoutFor Element(string type, LocalizedString name, LocalizedString description, Func<LayoutContext, LocalizedString> display, Func<LayoutContext, IEnumerable<LayoutComponentResult>, dynamic> render, string form = null) {
            Types.Add(new LayoutDescriptor { Type = type, Name = name, Description = description, Category = _category, Display = display, Render = render, Form = form });
            return this;
        }
    }
}