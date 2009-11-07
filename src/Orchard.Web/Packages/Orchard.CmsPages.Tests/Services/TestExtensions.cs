using Orchard.CmsPages.Models;
using Orchard.CmsPages.Services;
using Orchard.CmsPages.ViewModels;

namespace Orchard.CmsPages.Tests.Services {
    public static class TestExtensions {
        public static PageRevision CreateAndPublishPage(this IPageManager manager, string slug, string title) {
            var revision = manager.CreatePage(new PageCreateViewModel {Slug = slug, Title = title});
            manager.Publish(revision, new PublishOptions());
            return revision;
        }
    }
}