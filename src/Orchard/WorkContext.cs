using System;
using Autofac;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI;

namespace Orchard {
    public abstract class WorkContext {
        public IPage CurrentPage {
            get { return State<IPage>(); }
        }
        public ISite CurrentSite {
            get { return State<ISite>(); }
        }
        public IUser CurrentUser {
            get { return State<IUser>(); }
        }

        public abstract T Service<T>();
        public abstract T State<T>();
    }
}