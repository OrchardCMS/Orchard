using Orchard.CmsPages.Models;
using Orchard.CmsPages.Services;
using Orchard.CmsPages.ViewModels;

namespace Orchard.Tests.Packages.Pages.Services {
    public static class TestExtensions {
        public static PageRevision CreateAndPublishPage(this IPageManager manager, string slug, string title) {
            var revision = manager.CreatePage(new CreatePageParams(title, slug, null));
            manager.Publish(revision, new PublishOptions());
            return revision;
        }
    }
}