using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.Security;

namespace Orchard.Users.Services {
    public class DefaultUserManagementActionsProvider : IUserManagementActionsProvider {
        public IEnumerable<Func<HtmlHelper, MvcHtmlString>> UserActionLinks(IUser user) {
            yield break;
        }
    }
}