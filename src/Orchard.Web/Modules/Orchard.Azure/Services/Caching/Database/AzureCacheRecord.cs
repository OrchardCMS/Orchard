using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Azure.Services.Caching.Database.Models {

    /// <summary>
    /// Fake record in order to force the mappings to be updated
    /// once the modules is enabled/disabled.
    /// </summary>
    public class AzureCacheRecord {
        public virtual int Id {
            get;
            set;
        }
    }
}