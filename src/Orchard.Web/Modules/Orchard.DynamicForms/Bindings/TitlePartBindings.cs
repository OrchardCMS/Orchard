using Orchard.Core.Title.Models;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;

namespace Orchard.DynamicForms.Bindings {
    public class TitlePartBindings : Component, IBindingProvider {
        public void Describe(BindingDescribeContext context) {
            context.For<TitlePart>()
                .Binding("Title", (part, s) => part.Title = s);
        }
    }
}