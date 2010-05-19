using System.Web.Mvc;
using Orchard.ContentManagement.MetaData.Services;
using Orchard.Themes;
using Orchard.Data;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement.MetaData.Records;
using Orchard.MetaData.ViewModels;

namespace Orchard.MetaData.Controllers
{
    [Themed]
    public class MetaDataController : Controller
    {
        private readonly IRepository<ContentTypeRecord> _ctps;
        private readonly IContentTypeService _contentTypeService;

        public MetaDataController(IRepository<ContentTypeRecord> ctps, IContentTypeService contentTypeService){
            _ctps = ctps;
            _contentTypeService = contentTypeService;
        }

        //
        // GET: /ContentTypeList/

        public ActionResult ContentTypeList(string id) {
            var contentTypes = _contentTypeService.GetContentTypes();
            var contentTypePartNames = _contentTypeService.GetContentTypePartNames();

            var model = new ContentTypesIndexViewModel();

            foreach(ContentTypeRecord contentType in contentTypes) {
                var contentTypeEntry = new ContentTypeEntry() {Name = contentType.Name,DisplayName = contentType.Name};
                
                if (contentType.Name==id) {
                    foreach(ContentTypePartNameRecord contentTypePartNameRecord in contentTypePartNames) {
                        var contentTypePartEntry = new ContentTypePartEntry() { Name = contentTypePartNameRecord.PartName };
                        foreach(var contentTypePartEntryTest in contentType.ContentParts) {
                            if (contentTypePartEntryTest.PartName==contentTypePartEntry.Name) {
                                contentTypePartEntry.Selected = true;
                            }
                        }
                        model.ContentTypeParts.Add(contentTypePartEntry);
                    }
                    model.SelectedContentType = contentTypeEntry;
                }
                model.ContentTypes.Add(contentTypeEntry);
            };

            return View(model);
        }


        //
        // POST: /ContentTypeList/Save
        [HttpPost]
        public ActionResult Save(string id, FormCollection collection)
        {
            ContentTypeRecord contentTypeRecord = _contentTypeService.GetContentTypeRecord(id);
            while (contentTypeRecord.ContentParts.Count>0) {
                _contentTypeService.UnMapContentTypeToContentPart(contentTypeRecord.Name, contentTypeRecord.ContentParts[0].PartName);
            }
            //foreach(var contentTypePartNameRecord in contentTypeRecord.ContentParts) {
            //    _contentTypeService.UnMapContentTypeToContentPart(contentTypeRecord.Name,contentTypePartNameRecord.PartName);
            //}
            foreach(var formKey in collection.AllKeys) {
                if (formKey.Contains("part_")) {
                    var partName = formKey.Replace("part_", "");
                    _contentTypeService.MapContentTypeToContentPart(contentTypeRecord.Name,partName);
                }
            }

            return RedirectToAction("ContentTypeList", new { id = id });

            
        }
     
        
    }
}
