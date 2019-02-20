using Orchard.ContentManagement;

namespace Orchard.Projections {
    public static class QueryVersionScopeOptionsExtensions {
        public static VersionOptions ToVersionOptions(this QueryVersionScopeOptions scope) {
            switch (scope) {
                case QueryVersionScopeOptions.Latest:
                    return VersionOptions.Latest;
                case QueryVersionScopeOptions.Draft:
                    return VersionOptions.Draft;
                default:
                    return VersionOptions.Published;
            }
        }

        public static string ToVersionedFieldIndexColumnName(this QueryVersionScopeOptions scope) {
            switch (scope) {
                case QueryVersionScopeOptions.Latest:
                case QueryVersionScopeOptions.Draft:
                    return "LatestValue";
                default:
                    return "Value";
            }
        }
    }
}