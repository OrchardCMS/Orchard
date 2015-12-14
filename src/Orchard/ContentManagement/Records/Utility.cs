using System;

namespace Orchard.ContentManagement.Records {
    public static class Utility {
        public static bool IsPartRecord(Type type) {
            return type.IsSubclassOf(typeof(ContentPartRecord)) && !type.IsSubclassOf(typeof(ContentPartVersionRecord));
        }

        public static bool IsPartVersionRecord(Type type) {
            return type.IsSubclassOf(typeof(ContentPartVersionRecord));
        }
    }
}
