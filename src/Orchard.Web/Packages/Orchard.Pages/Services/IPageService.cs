using System.Collections.Generic;
using Orchard.Pages.Models;

namespace Orchard.Pages.Services {
    public interface IPageService : IDependency {
        IEnumerable<Page> Get();
        Page Get(string slug);
        Page GetPageOrDraft(string slug);
        Page New();
        Page Create(bool publishNow);
        void Delete(Page page);
        void Publish(Page page);
    }
}