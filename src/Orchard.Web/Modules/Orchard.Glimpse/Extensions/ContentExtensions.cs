using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Users.Models;
using Orchard.Widgets.Models;

namespace Orchard.Glimpse.Extensions {
    public static class ContentExtensions {
        public static string GetContentName(this IContent content) {
            if (content.Has<TitlePart>()) {
                return content.As<TitlePart>().Title;
            }
            if (content.Has<WidgetPart>()) {
                return content.As<WidgetPart>().Title;
            }
            if (content.Has<UserPart>()) {
                return content.As<UserPart>().UserName;
            }
            if (content.Has<LayerPart>()) {
                return content.As<LayerPart>().Name;
            }

            return "Unknown";
        }
    }
}