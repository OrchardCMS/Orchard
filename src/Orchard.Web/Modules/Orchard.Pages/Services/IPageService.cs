using System;
using System.Collections.Generic;
using Orchard.Pages.Models;

namespace Orchard.Pages.Services {
    public interface IPageService : IDependency {
        IEnumerable<Page> Get();
        IEnumerable<Page> Get(PageStatus status);
        Page Get(string slug);
        Page Get(int id);
        Page GetPageOrDraft(string slug);
        Page GetPageOrDraft(int id);
        Page GetLatest(string slug);
        Page GetLatest(int id);
        void Delete(Page page);
        void Publish(Page page);
        void Publish(Page page, DateTime scheduledPublishUtc);
        void Unpublish(Page page);
        DateTime? GetScheduledPublishUtc(Page page);
    }

    public enum PageStatus {
        All,
        Published,
        Offline
    }
}