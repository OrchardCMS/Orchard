using System.Web;
using Orchard.ContentManagement;

namespace Orchard.Localization.Services {
    public interface ICultureFilter : IDependency {
        IContentQuery<ContentItem> FilterCulture(IContentQuery<ContentItem> query, string cultureName);
    }
}
