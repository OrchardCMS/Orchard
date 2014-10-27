using Orchard.ContentManagement;

namespace Orchard.Layouts.Services {
    public class DescribeElementsContext {
        public IContent Content { get; set; }
        public static DescribeElementsContext Empty = new DescribeElementsContext();
    }
}