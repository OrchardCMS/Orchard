using System.Web;
using NHibernate.Connection;
using NHibernate.Driver;

namespace Orchard.Glimpse.ADO {
    public class GlimpseConnectionProvider : DriverConnectionProvider, IConnectionProvider {
        public new IDriver Driver {
            get {
                var originalDriver = base.Driver;

                if (HttpContext.Current == null || originalDriver is GlimpseDriver) {
                    return originalDriver;
                }

                return new GlimpseDriver(originalDriver);
            }
        }
    }
}