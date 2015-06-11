using Orchard.Utility.Extensions;

namespace Orchard.Core.Containers.Services {
    public interface IListViewProvider : IDependency {
        string Name { get; }
        string DisplayName { get; }
        int Priority { get; }
        dynamic BuildDisplay(BuildListViewDisplayContext context);
    }

    public abstract class ListViewProviderBase : IListViewProvider {
        public virtual string Name { get { return GetType().Name.Replace("ListView", ""); } }
        public virtual string DisplayName { get { return Name.CamelFriendly(); } }
        public virtual int Priority { get { return 0; } }
        public abstract dynamic BuildDisplay(BuildListViewDisplayContext context);
    }
}