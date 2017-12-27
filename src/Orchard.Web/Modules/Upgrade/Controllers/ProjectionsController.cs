using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Data;
using Orchard.Environment.Features;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Projections.Handlers;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using Upgrade.ViewModels;

namespace Upgrade.Controllers {
    [Admin]
    public class ProjectionsController : Controller {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IFeatureManager _featureManager;
        private readonly Lazy<IEnumerable<IContentHandler>> _handlers;
        private readonly ITransactionManager _transactionManager;

        private const int BATCH = 50;

        public ProjectionsController(
            IContentDefinitionManager contentDefinitionManager,
            IOrchardServices orchardServices,
            ITransactionManager transactionManager,
            IFeatureManager featureManager,
            Lazy<IEnumerable<IContentHandler>> handlers) {
            _contentDefinitionManager = contentDefinitionManager;
            _orchardServices = orchardServices;
            _transactionManager = transactionManager;
            _featureManager = featureManager;
            _handlers = handlers;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index() {
            var viewModel = new MigrateViewModel { ContentTypes = new List<ContentTypeEntry>() };
            foreach (var contentType in _contentDefinitionManager.ListTypeDefinitions().OrderBy(c => c.Name)) {
                // only display parts with fields
                if (contentType.Parts.Any(x => x.PartDefinition.Fields.Any())) {
                    viewModel.ContentTypes.Add(new ContentTypeEntry { ContentTypeName = contentType.Name });
                }
            }

            if (!viewModel.ContentTypes.Any()) {
                _orchardServices.Notifier.Warning(T("There are no content types with custom fields"));
            }

            if (!_featureManager.GetEnabledFeatures().Any(x => x.Id == "Orchard.Fields")) {
                _orchardServices.Notifier.Warning(T("You need to enable Orchard.Fields in order to migrate current fields."));
            }

            return View(viewModel);
        }

        //[HttpPost, ActionName("Index")]
        //public ActionResult IndexPOST() {
        //    if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to update fields' indexes.")))
        //        return new HttpUnauthorizedResult();

        //    // Get all ContentTypes with fields and Updates the LatestValue if necessary
        //    var contentTypesWithFields = _contentDefinitionManager.ListTypeDefinitions().OrderBy(c => c.Name).Where(w => w.Parts.Any(x => x.PartDefinition.Fields.Any()));
        //    foreach (var contentTypeWithFields in contentTypesWithFields) {
        //        var contents = _orchardServices.ContentManager.HqlQuery().ForType(contentTypeWithFields.Name).ForVersion(VersionOptions.Latest).List();
        //        foreach (var content in contents) {
        //            _handlers.Value.Where(x => x.GetType() == typeof(FieldIndexPartHandler)).Invoke(handler => handler.Updated(new UpdateContentContext(content)), Logger);
        //            _transactionManager.RequireNew();
        //        }
        //    }
        //    _orchardServices.Notifier.Information(T("Fields latest values were indexed successfully"));

        //    return RedirectToAction("Index");
        //}

        [HttpPost]
        public JsonResult MigrateLatestValue(int id) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner))
                throw new AuthenticationException("");
            var contentTypeNamesWithFields = _contentDefinitionManager.ListTypeDefinitions()
                .Where(w => w.Parts.Any(x => x.PartDefinition.Fields.Any()))
                .Select(x => x.Name)
                .ToArray();

            var contents = _orchardServices.ContentManager.HqlQuery().ForVersion(VersionOptions.Latest)
                .Where(ci => ci.ContentItem(), cix => cix.Gt("Id", id))
                .OrderBy(alias => alias.ContentItem(), order => order.Asc("Id"))
                .Slice(0, BATCH).ToList();
            var lastContentItemId = id;

            foreach (var content in contents) {
                if (contentTypeNamesWithFields.Contains(content.ContentType)) {
                    _handlers.Value.Where(x => x.GetType() == typeof(FieldIndexPartHandler)).Invoke(handler => handler.Updated(new UpdateContentContext(content)), Logger);
                }
                lastContentItemId = content.Id;
            }

            return new JsonResult { Data = lastContentItemId };
        }

    }
}