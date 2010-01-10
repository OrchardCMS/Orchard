using System.Collections.Generic;
using Orchard.Pages.Models;

namespace Orchard.Pages.Services {
    public interface IPageService : IDependency {
        IEnumerable<Page> Get();
        IEnumerable<Page> Get(PageStatus status);
        Page Get(string slug);
        Page GetPageOrDraft(string slug);
        Page GetLatest(string slug);
        Page GetLatest(int id);
        Page New();
        Page Create(bool publishNow);
        void Delete(Page page);
        void Publish(Page page);
        void Unpublish(Page page);
    }

    public enum PageStatus {
        All,
        Published,
        Offline
    }
}