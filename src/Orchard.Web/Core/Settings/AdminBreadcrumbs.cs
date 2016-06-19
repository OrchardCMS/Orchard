using Orchard.Core.Contents.Navigation;
using Orchard.UI.Navigation;

namespace Orchard.Core.Settings {
    public class AdminBreadcrumbs : AdminBreadcrumbsProvider {
        public const string Name = "Orchard.Core.Settings.AdminBreadcrumbs";
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
                 settings.Add(T("Supported Cultures"), cultures => cultures.Action("Index", "Admin", new { area = "Settings", groupInfoId = "Culture" }));

                foreach (var groupInfo in _orchardServices.ContentManager.GetEditorGroupInfos(_orchardServices.WorkContext.CurrentSite.ContentItem)) {
                    var info = groupInfo;
                    settings.Add(info.Name, item => item.Action("Index", "Admin", new { area = "Settings", groupInfoId = info.Id }));
                }
            });
        }
    }
}