using System;
using System.Web;
using Autofac;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI;

namespace Orchard {
    public abstract class WorkContext {
        public HttpContextBase HttpContext {
            get { return State<HttpContextBase>(); }
        }
        public IPage CurrentPage {
            get { return State<IPage>(); }
        }
        public ISite CurrentSite {
            get { return State<ISite>(); }
        }
        public IUser CurrentUser {
            get { return State<IUser>(); }
        }

        public abstract T Resolve<T>();
        public abstract T State<T>();
    }
}