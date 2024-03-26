using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Projections.Services;
using Orchard.Recipes.Services;

namespace Orchard.Projections.Providers.Builders {
    public class MemberBindingsStep : RecipeBuilderStep {
        private readonly IMemberBindingProvider _memberBindingProvider;
        public MemberBindingsStep(IMemberBindingProvider memberBindingProvider) {
            _memberBindingProvider = memberBindingProvider;
        }

        public override string Name {
            get { return "MemberBindings"; }
        }

        public override LocalizedString DisplayName {
            get { return T("Member Bindings"); }
        }

        public override LocalizedString Description {
            get { return T("Exports query member bindings."); }
        }

        public override int Priority {
            get { return 25; }
        }

        public override int Position {
            get { return 25; }
        }

        public override void Build(BuildContext context) {
            var memberBindings = new XElement("MemberBindings");
            context.RecipeDocument.Element("Orchard").Add(memberBindings);

            var bindingBuilder = new BindingBuilder();
            _memberBindingProvider.GetMemberBindings(bindingBuilder);

            foreach (var bindingItem in bindingBuilder.Build()) {
                var declaringType = bindingItem.Property.DeclaringType;

                var memberBinding = new XElement("MemberBinding",
                    new XAttribute("Type", declaringType.FullName),
                    new XAttribute("Member", bindingItem.Property.Name),
                    new XAttribute("Description", bindingItem.Description),
                    new XAttribute("DisplayName", bindingItem.DisplayName));

                memberBindings.Add(memberBinding);
            }
        }
    }
}