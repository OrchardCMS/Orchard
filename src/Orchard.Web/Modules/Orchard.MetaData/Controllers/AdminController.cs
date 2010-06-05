using System.Web.Mvc;
using Orchard.ContentManagement.MetaData;
using Orchard.Localization;
using Orchard.MetaData.ViewModels;

namespace Orchard.MetaData.Controllers {
    
    public class AdminController : Controller {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        public IOrchardServices Services { get; set; }

        public AdminController(IOrchardServices services, IContentDefinitionManager contentDefinitionManager) {
            _contentDefinitionManager = contentDefinitionManager;
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        //
        // GET: /ContentTypeList/

        public ActionResult ContentTypeList(string id) {

            if (!Services.Authorizer.Authorize(Permissions.ManageMetaData, T("Not allowed to manage MetaData")))
                return new HttpUnauthorizedResult();

            var contentTypes = _contentDefinitionManager.ListTypeDefinitions();
            var contentParts = _contentDefinitionManager.ListPartDefinitions();

            var model = new ContentTypesIndexViewModel();

            foreach (var contentType in contentTypes) {
                var contentTypeEntry = new ContentTypeEntry { Name = contentType.Name, DisplayName = contentType.Name };

                if (contentType.Name == id) {
                    foreach (var contentTypePartNameRecord in contentParts) {
                        var contentTypePartEntry = new ContentTypePartEntry { Name = contentTypePartNameRecord.Name };
                        foreach (var contentTypePartEntryTest in contentType.Parts) {
                            if (contentTypePartEntryTest.PartDefinition.Name == contentTypePartEntry.Name) {
                                contentTypePartEntry.Selected = true;
                            }
                        }
                        model.ContentTypeParts.Add(contentTypePartEntry);
                    }
                    model.SelectedContentType = contentTypeEntry;
                }
                model.ContentTypes.Add(contentTypeEntry);
            }
            return View(model);
        }


        //
        // POST: /ContentTypeList/Save
        [HttpPost]
        public ActionResult Save(string id, FormCollection collection) {
            if (!Services.Authorizer.Authorize(Permissions.ManageMetaData, T("Not allowed to manage MetaData")))
                return new HttpUnauthorizedResult();
            
            var existingDefinition = _contentDefinitionManager.GetTypeDefinition(id);

            _contentDefinitionManager.AlterTypeDefinition(id, alter => {
                foreach(var part in existingDefinition.Parts) {
                    alter.RemovePart(part.PartDefinition.Name);
                }
                foreach (var formKey in collection.AllKeys) {
                    if (formKey.Contains("part_")) {
                        var partName = formKey.Replace("part_", "");
                        alter.WithPart(partName);
                   }
                }

            });

            return RedirectToAction("ContentTypeList", new { id });


        }


    }
}
