using Orchard.Core.Contents.Navigation;
using Orchard.UI.Navigation;

namespace Orchard.OutputCache {
    public class AdminBreadcrumbs : AdminBreadcrumbsProvider {
        public const string Name = "Orchard.OutputCache.AdminBreadcrumbs";
        private readonly IOrchardServices _orchardServices;

        public AdminBreadcrumbs(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public override string MenuName {
            get { return Name; }
        }

        protected override void AddItems(NavigationItemBuilder root) {
            root.Add(T("Manage Settings"), settings => {
                settings.Action("Index", "Admin", new { area = "Settings", groupInfoId = "Index" });
                settings.Add(T("Cache"), cache => cache
                    .Action("Index", "Admin", new { area = "Orchard.OutputCache" })
                    .Add(T("Statistics"), statistics => statistics.Action("Statistics", "Admin", new { area = "Orchard.OutputCache" })));
            });
        }
    }
}