using System;
using System.Collections.Generic;
using Orchard.Localization;

namespace Orchard.Rules.Models {
    public class DescribeActionFor {
        private readonly string _category;

        public DescribeActionFor(string category, LocalizedString name, LocalizedString description) {
            Types = new List<ActionDescriptor>();
            _category = category;
            Name = name;
            Description = description;
        }

        public LocalizedString Name { get; private set; }
        public LocalizedString Description { get; private set; }
        public List<ActionDescriptor> Types { get; private set; }

        public DescribeActionFor Element(string type, LocalizedString name, LocalizedString description, Func<ActionContext, bool> action, Func<ActionContext, LocalizedString> display, string form = null) {
            Types.Add(new ActionDescriptor { Type = type, Name = name, Description = description, Category = _category, Action = action, Display = display, Form = form });
            return this;
        }
    }
}