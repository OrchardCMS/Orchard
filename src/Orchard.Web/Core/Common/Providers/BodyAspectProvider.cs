using System;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Records;
using Orchard.Data;
using Orchard.Models.Driver;

namespace Orchard.Core.Common.Providers {
    public class BodyAspectProvider : ContentProvider {
        public BodyAspectProvider(IRepository<BodyRecord> bodyRepository) {            
            Filters.Add(new StorageFilter<BodyRecord>(bodyRepository));
            OnGetEditors<BodyAspect>();
        }

        private void OnGetEditors<TPart>() {
            
        }
    }
}