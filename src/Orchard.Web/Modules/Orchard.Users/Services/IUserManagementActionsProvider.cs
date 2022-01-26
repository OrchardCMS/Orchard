using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Orchard.Security;

namespace Orchard.Users.Services {
    public interface IUserManagementActionsProvider : IDependency {
        // Using a delegate so implementations don't have to build/figure out
        // their own HtmlHelper
        IEnumerable<Func<HtmlHelper, MvcHtmlString>> UserActionLinks(IUser user);
    }
}
