using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using ClaySharp;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.DisplayManagement {
    public interface IDisplayManager : ISingletonDependency {
        object Execute(Shape shape, ViewContext viewContext, IViewDataContainer viewDataContainer);
    }
}
