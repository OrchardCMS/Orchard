using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using UpgradeTo14.ViewModels;

namespace UpgradeTo14.Controllers {
    [Admin]
    public class FieldController : Controller {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IOrchardServices _orchardServices;
        private readonly ISessionFactoryHolder _sessionFactoryHolder;

        public FieldController(
            IContentDefinitionManager contentDefinitionManager,
            IOrchardServices orchardServices,
            ISessionFactoryHolder sessionFactoryHolder) {
            _contentDefinitionManager = contentDefinitionManager;
            _orchardServices = orchardServices;
            _sessionFactoryHolder = sessionFactoryHolder;
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
                        contents = _orchardServices.ContentManager.HqlQuery().ForType(contentType).Slice(count, 100);

                        foreach (ContentItem content in contents) {

                            // copy data to current version record
                            content.VersionRecord.Data = content.Record.Data;
                            var draft = _orchardServices.ContentManager.Get(content.Id, VersionOptions.Draft);

                            if(draft != null) {
                                draft.VersionRecord.Data = content.Record.Data;
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
