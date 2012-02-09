using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web.Mvc;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.Autoroute.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
using UpgradeTo14.ViewModels;

namespace UpgradeTo14.Controllers {
    public class AdminController : Controller {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IOrchardServices _orchardServices;
        private readonly ISessionFactoryHolder _sessionFactoryHolder;
        private readonly ShellSettings _shellSettings;
        private readonly IAutorouteService _autorouteService;

        public AdminController(
            IContentDefinitionManager contentDefinitionManager,
            IOrchardServices orchardServices,
            ISessionFactoryHolder sessionFactoryHolder,
            ShellSettings shellSettings,
            IAutorouteService autorouteService) {
            _contentDefinitionManager = contentDefinitionManager;
            _orchardServices = orchardServices;
            _sessionFactoryHolder = sessionFactoryHolder;
            _shellSettings = shellSettings;
            _autorouteService = autorouteService;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            var viewModel = new MigrateViewModel { ContentTypes = new List<ContentTypeEntry>() };
            foreach (var contentType in _contentDefinitionManager.ListTypeDefinitions().OrderBy(c => c.Name)) {
                // only display routeparts
                if (contentType.Parts.Any(x => x.PartDefinition.Name == "RoutePart")) {
                    viewModel.ContentTypes.Add(new ContentTypeEntry {ContentTypeName = contentType.Name});
                }
            }

            if(!viewModel.ContentTypes.Any()) {
                _orchardServices.Notifier.Warning(T("There are no content types with RoutePart"));
            }

            return View(viewModel);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to migrate routes.")))
                return new HttpUnauthorizedResult();

            var viewModel = new MigrateViewModel { ContentTypes = new List<ContentTypeEntry>() };

            if(TryUpdateModel(viewModel)) {
                var contentTypesToMigrate = viewModel.ContentTypes.Where(c => c.IsChecked).Select(c => c.ContentTypeName);

                var sessionFactory = _sessionFactoryHolder.GetSessionFactory();
                var session = sessionFactory.OpenSession();

                foreach (var contentType in contentTypesToMigrate) {

                    // migrating parts
                    _contentDefinitionManager.AlterTypeDefinition(contentType,
                        builder => builder
                                        .WithPart("AutoroutePart")
                                        .WithPart("TitlePart"));

                    // force the first object to be reloaded in order to get a valid AutoroutePart
                    _orchardServices.ContentManager.Flush();
                    _orchardServices.ContentManager.Clear();

                    var count = 0;
                    var isContainable = false;
                    IEnumerable<ContentItem> contents;

                    do {
                        contents = _orchardServices.ContentManager.HqlQuery().ForType(contentType).Slice(count, 100);

                        foreach (dynamic content in contents) {
                            var autoroutePart = ((ContentItem)content).As<AutoroutePart>();
                            var titlePart = ((ContentItem) content).As<TitlePart>();
                            var commonPart = ((ContentItem) content).As<CommonPart>();
                            
                            if(commonPart != null && commonPart.Container != null) {
                                isContainable = true;
                            }

                            using (new TransactionScope(TransactionScopeOption.RequiresNew)) {
                                var command = session.Connection.CreateCommand();
                                command.CommandText = string.Format("SELECT Title, Path FROM {0} WHERE ContentItemRecord_Id = {1}", GetPrefixedTableName("Routable_RoutePartRecord"), autoroutePart.ContentItem.Id);
                                var reader = command.ExecuteReader();
                                reader.Read();
                                var title = reader.GetString(0);
                                var path = reader.GetString(1);
                                reader.Close();

                                autoroutePart.DisplayAlias = path;
                                titlePart.Title = title;
                            }

                            _autorouteService.PublishAlias(autoroutePart);
                            count++;
                        }

                        _orchardServices.ContentManager.Flush();
                        _orchardServices.ContentManager.Clear();

                    } while (contents.Any());
 
                    _contentDefinitionManager.AlterTypeDefinition(contentType, builder => builder.RemovePart("RoutePart"));
                    
                    var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
                    if (isContainable || typeDefinition.Parts.Any(x => x.PartDefinition.Name == "ContainablePart")) {
                        _autorouteService.CreatePattern(contentType, "Container and Title", "{Content.Container.Path}/{Content.Slug}", "my-container/a-sample-title", true);
                    }
                    else {
                        _autorouteService.CreatePattern(contentType, "Title", "{Content.Slug}", "my-sample-title", true);    
                    }

                    _orchardServices.Notifier.Information(T("{0} was migrated successfully", contentType));
                }
            }

            return RedirectToAction("Index");
        }

        private string GetPrefixedTableName(string tableName) {
            if (string.IsNullOrWhiteSpace(_shellSettings.DataTablePrefix)) {
                return tableName;
            }

            return _shellSettings.DataTablePrefix + "_" + tableName;
        }
    }
}
