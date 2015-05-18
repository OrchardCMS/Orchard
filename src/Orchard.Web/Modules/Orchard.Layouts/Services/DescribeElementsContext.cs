using Orchard.ContentManagement;

namespace Orchard.Layouts.Services {
    public class DescribeElementsContext {
        public IContent Content { get; set; }
        public string CacheVaryParam { get; set; }

        public static readonly DescribeElementsContext Empty = new DescribeElementsContext();
    }
}