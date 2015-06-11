using System.Collections.Generic;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Framework.Harvesters {
    public interface ICategoryProvider : IDependency {
        IEnumerable<Category> GetCategories();
    }
}