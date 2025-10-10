using Orchard.Utility.Extensions;

namespace Orchard.Indexing.Helpers {
    public static class IndexingHelpers {
        public static bool IsValidIndexName(string name) =>
            !string.IsNullOrWhiteSpace(name) && name.ToSafeName() == name;
    }
}
