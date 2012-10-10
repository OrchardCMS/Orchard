using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Features;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using UpgradeTo16.ViewModels;

namespace UpgradeTo16.Controllers {
    [Admin]
    public class FieldController : Controller {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IFeatureManager _featureManager;

        public FieldController(
            IContentDefinitionManager contentDefinitionManager,
            IOrchardServices orchardServices,
            IFeatureManager featureManager) {
            _contentDefinitionManager = contentDefinitionManager;
            _orchardServices = orchardServices;
            _featureManager = featureManager;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            var viewModel = new MigrateViewModel { ContentTypes = new List<ContentTypeEntry>() };
            foreach (var contentType in _contentDefinitionManager.ListTypeDefinitions().OrderBy(c => c.Name)) {
                // only display parts with fields
                if (contentType.Parts.Any(x => x.PartDefinition.Fields.Any())) {
                    viewModel.ContentTypes.Add(new ContentTypeEntry {ContentTypeName = contentType.Name});
                }
            }

            if(!viewModel.ContentTypes.Any()) {
                _orchardServices.Notifier.Warning(T("There are no content types with custom fields"));
            }

            if(!_featureManager.GetEnabledFeatures().Any(x => x.Id == "Orchard.Fields")) {
                _orchardServices.Notifier.Warning(T("You need to enable Orchard.Fields in order to migrate current fields. Then you can safely remove Contrib.DateTimeField and Contrib.MediaPickerField."));
            }

            return View(viewModel);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to migrate fields.")))
                return new HttpUnauthorizedResult();

            var viewModel = new MigrateViewModel { ContentTypes = new List<ContentTypeEntry>() };

            if(TryUpdateModel(viewModel)) {
                var contentTypesToMigrate = viewModel.ContentTypes.Where(c => c.IsChecked).Select(c => c.ContentTypeName);

                foreach (var contentType in contentTypesToMigrate) {

                    _orchardServices.ContentManager.Flush();
                    _orchardServices.ContentManager.Clear();

                    var count = 0;
                    IEnumerable<ContentItem> contents;

                    do {
                        contents = _orchardServices.ContentManager.HqlQuery().ForType(contentType).ForVersion(VersionOptions.Latest).Slice(count, 100);

                        foreach (ContentItem content in contents) {

                            if((content.Record.Data ?? "").Length > (content.VersionRecord.Data ?? "").Length) {
                             
                                var draft = _orchardServices.ContentManager.Get(content.Id, VersionOptions.Draft);

                                if(draft != null) {
                                    draft.VersionRecord.Data = content.Record.Data;
                                }
                                else {
                                    content.VersionRecord.Data = content.Record.Data;
                                }
                            }

                            count++;
                        }

                        _orchardServices.ContentManager.Flush();
                        _orchardServices.ContentManager.Clear();

                    } while (contents.Any());
 
                    _orchardServices.Notifier.Information(T("{0} fields were migrated successfully", contentType));
                }
            }

            return RedirectToAction("Index");
        }
    }
}
