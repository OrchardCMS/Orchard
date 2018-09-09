using Glimpse.Core.Message;

namespace Orchard.Glimpse {
    public class TimelineCategories {
        public static TimelineCategoryItem Cache { get { return new TimelineCategoryItem("Cache", "#154B87", "#154B87"); } }
        public static TimelineCategoryItem Layers { get { return new TimelineCategoryItem("Layers", "#E6261D", "#E6261D"); } }
        public static TimelineCategoryItem Parts { get { return new TimelineCategoryItem("Parts", "#169871", "#169871"); } }
        public static TimelineCategoryItem ContentManager { get { return new TimelineCategoryItem("Content Manager", "#603182", "#603182"); } }
        public static TimelineCategoryItem Shapes { get { return new TimelineCategoryItem("Shapes", "#bcbb27", "#bcbb27"); } }
        public static TimelineCategoryItem Authorizer { get { return new TimelineCategoryItem("Authorizer", "#da7520", "#da7520"); } }
        public static TimelineCategoryItem Widgets { get { return new TimelineCategoryItem("Widgets", "#84DCC6", "#84DCC6"); } }
    }
}