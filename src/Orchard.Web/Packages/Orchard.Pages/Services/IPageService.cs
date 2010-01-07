using System.Collections.Generic;
using Orchard.Pages.Models;

namespace Orchard.Pages.Services {
    public interface IPageService : IDependency {
        IEnumerable<Page> Get();
        Page Get(string slug);
        void Delete(Page page);
    }
}