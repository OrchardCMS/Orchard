using Orchard.UI.Navigation;

namespace Orchard.Core.Contents.Navigation {
    public abstract class AdminBreadcrumbsProvider : Component, INavigationProvider {
        public abstract string MenuName { get; }

        public virtual void GetNavigation(NavigationBuilder builder) {

            builder.Add(T("Home"), root => {
                root.Url("~/Admin");

                AddItems(root);
            });
        }
        
        protected virtual void AddItems(NavigationItemBuilder home) {
        }
    }
}