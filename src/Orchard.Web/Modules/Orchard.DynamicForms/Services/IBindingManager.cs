using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.DynamicForms.Services.Models;

namespace Orchard.DynamicForms.Services {
    public interface IBindingManager : IDependency {
        IEnumerable<BindingContext> DescribeBindingContexts();
        IEnumerable<ContentPartBindingDescriptor> DescribeBindingsFor(ContentTypeDefinition contentTypeDefinition);
    }
}