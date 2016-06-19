using System.Web.Routing;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Contents.Navigation;
using Orchard.Localization;
using Orchard.UI.Navigation;
using Orchard.Utility.Extensions;

namespace Orchard.ContentTypes {
    public class AdminBreadcrumbs : AdminBreadcrumbsProvider {
        public const string Name = "Orchard.ContentTypes.AdminBreadcrumbs";
        private readonly IOrchardServices _orchardServices;

        public AdminBreadcrumbs(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public override string MenuName {
            get { return Name; }
        }

        protected override void AddItems(NavigationItemBuilder root) {
            var context = _orchardServices.WorkContext.Layout.Breadcrumbs.Context as RouteValueDictionary;

            root.Add(T("Content Types"), contentTypes => {
                contentTypes.Action("Index", "Admin", new { area = "Orchard.ContentTypes" });
                contentTypes.Add(T("New"), createContentType => createContentType.Action("Create", "Admin", new { area = "Orchard.ContentTypes" }));
    
                var currentContentType = context != null ? context["CurrentContentType"] as ContentTypeDefinition : default(ContentTypeDefinition);

                if(currentContentType != null) {
                    contentTypes.Add(new LocalizedString(currentContentType.DisplayName), contentType => contentType
                        .Action("Edit", "Admin", new { area = "Orchard.ContentTypes", id = currentContentType.Name })
                        .Add(T("Add Parts"), addParts => addParts.Action("AddPartsTo", "Admin", new { area = "Orchard.ContentTypes", id = currentContentType.Name }))
                        .Add(T("Add Field"), addField => addField.Action("AddFieldTo", "Admin", new { area = "Orchard.ContentTypes", id = currentContentType.Name }))
                        .Add(T("Edit Field"), editField => editField.Action("EditField", "Admin", new { area = "Orchard.ContentTypes", id = currentContentType.Name }))
                        .Add(T("Placement"), placement => placement.Action("EditPlacement", "Admin", new { area = "Orchard.ContentTypes", id = currentContentType.Name })));
                }
            });

            root.Add(T("Content Parts"), contentParts => {
                contentParts.Action("ListParts", "Admin", new { area = "Orchard.ContentTypes" });
                contentParts.Add(T("New"), createContentPart => createContentPart.Action("CreatePart", "Admin", new { area = "Orchard.ContentTypes" }));

                var currentContentPart = context != null ? context["CurrentContentPart"] as ContentPartDefinition : default(ContentPartDefinition);

                if (currentContentPart != null) {
                    contentParts.Add(new LocalizedString(currentContentPart.Name.CamelFriendly()), contentPart => contentPart
                        .Action("EditPart", "Admin", new { area = "Orchard.ContentTypes", id = currentContentPart.Name })
                        .Add(T("Add Field"), addField => addField.Action("AddFieldTo", "Admin", new { area = "Orchard.ContentTypes", id = currentContentPart.Name }))
                        .Add(T("Edit Field"), editField => editField.Action("EditField", "Admin", new { area = "Orchard.ContentTypes", id = currentContentPart.Name })));
                }
            });
        }
    }
}