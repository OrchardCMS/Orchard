using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Containers.Models;
using Orchard.UI.Navigation;

namespace Orchard.Core.Containers.Services {
    public interface IListViewService : IDependency {
        IEnumerable<IListViewProvider> Providers { get; }
        IListViewProvider GetProvider(string name);
        IListViewProvider GetDefaultProvider();
    }

    public class ListViewService : IListViewService {
        private readonly IEnumerable<IListViewProvider> _providers;

        public ListViewService(IEnumerable<IListViewProvider> providers) {
            _providers = providers.OrderBy(x => x.Priority);
        }

        public IEnumerable<IListViewProvider> Providers {
            get { return _providers; }
        }

        public IListViewProvider GetProvider(string name) {
            return Providers.FirstOrDefault(x => x.Name == name);
        }

        public IListViewProvider GetDefaultProvider() {
            return Providers.First();
        }
    }

    public class BuildListViewDisplayContext {
        public dynamic New { get; set; }
        public IContentQuery<ContentItem> ContentQuery { get; set; }
        public Pager Pager { get; set; }
        public ContainerPart Container { get; set; }
        public string ContainerDisplayName { get; set; }
    }
}