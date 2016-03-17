using Orchard.ContentManagement;

namespace Orchard.UI.Navigation {
    public interface IMenuProvider : IDependency {
        void GetMenu(IContent menu, NavigationBuilder builder);
    }
}