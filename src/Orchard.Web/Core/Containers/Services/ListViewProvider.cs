using System;
using System.Threading.Tasks;
using Orchard.Utility.Extensions;

namespace Orchard.Core.Containers.Services {
    public interface IListViewProvider : IDependency {
        string Name { get; }
        string DisplayName { get; }
        int Priority { get; }
        [Obsolete("Use BuildDisplayAsync")]
        dynamic BuildDisplay(BuildListViewDisplayContext context);
        Task<dynamic> BuildDisplayAsync(BuildListViewDisplayContext context);
    }

    public abstract class ListViewProviderBase : IListViewProvider {
        public virtual string Name { get { return GetType().Name.Replace("ListView", ""); } }
        public virtual string DisplayName { get { return Name.CamelFriendly(); } }
        public virtual int Priority { get { return 0; } }

        public virtual dynamic BuildDisplay(BuildListViewDisplayContext context) {
            return null;
        }

        public virtual Task<dynamic> BuildDisplayAsync(BuildListViewDisplayContext context) {
            return Task.FromResult(BuildDisplay(context));
        }
    }
}