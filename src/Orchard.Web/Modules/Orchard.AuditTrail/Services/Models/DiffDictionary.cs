using System.Collections.Generic;

namespace Orchard.AuditTrail.Services.Models {
    public class DiffDictionary<TKey, TValue> : Dictionary<TKey, Diff<TValue>> {}
}