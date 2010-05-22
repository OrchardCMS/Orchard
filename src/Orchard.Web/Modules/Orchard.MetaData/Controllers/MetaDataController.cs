using System.Web.Mvc;
using Orchard.ContentManagement.MetaData.Services;
using Orchard.Localization;
using Orchard.MetaData.ViewModels;
using Orchard.UI.Admin;

namespace Orchard.MetaData.Controllers
{
    [Admin]
    public class MetaDataController : Controller
    {
        private readonly IContentTypeService _contentTypeService;
        public IOrchardServices Services { get; set; }

        public MetaDataController(IOrchardServices services, IContentTypeService contentTypeService)
        {
            _contentTypeService = contentTypeService;
            Services = services;
            T = NullLocalizer.Instance;
        }

        private Localizer T { get; set; }
        //
        // GET: /ContentTypeList/

        public ActionResult ContentTypeList(string id) {

            if (!Services.Authorizer.Authorize(Permissions.ManageMetaData, T("Not allowed to manage MetaData")))
                return new HttpUnauthorizedResult();

            var contentTypes = _contentTypeService.GetContentTypes();
            var contentTypePartNames = _contentTypeService.GetContentTypePartNames();

            var model = new ContentTypesIndexViewModel();

            foreach(var contentType in contentTypes) {
                var contentTypeEntry = new ContentTypeEntry {Name = contentType.Name,DisplayName = contentType.Name};
                
                if (contentType.Name==id) {
                    foreach(var contentTypePartNameRecord in contentTypePartNames) {
                        var contentTypePartEntry = new ContentTypePartEntry { Name = contentTypePartNameRecord.PartName };
                        foreach(var contentTypePartEntryTest in contentType.ContentParts) {
                            if (contentTypePartEntryTest.PartName.PartName==contentTypePartEntry.Name) {
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
        public ActionResult Save(string id, FormCollection collection)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageMetaData, T("Not allowed to manage MetaData")))
                return new HttpUnauthorizedResult();

            var contentTypeRecord = _contentTypeService.GetContentTypeRecord(id);
            //using a while loop because we are removing items from the collection
            while (contentTypeRecord.ContentParts.Count>0) {
                _contentTypeService.UnMapContentTypeToContentPart(contentTypeRecord.Name, contentTypeRecord.ContentParts[0].PartName.PartName);
            }
            foreach(var formKey in collection.AllKeys) {
                if (formKey.Contains("part_")) {
                    var partName = formKey.Replace("part_", "");
                    _contentTypeService.MapContentTypeToContentPart(contentTypeRecord.Name,partName);
                }
            }

            return RedirectToAction("ContentTypeList", new { id });

            
        }
     
        
    }
}
