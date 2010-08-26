using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ClaySharp;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.DisplayManagement {
    public interface IDisplayManager : IDependency {
        IHtmlString Execute(DisplayContext context);
    }
}
