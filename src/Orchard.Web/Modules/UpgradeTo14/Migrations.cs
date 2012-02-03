using System.Transactions;
using Orchard.Autoroute.Models;
using Orchard.Autoroute.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Data.Migration;
using Orchard.Environment.Configuration;

namespace UpgradeTo14 {
    public class UpdateTo14DataMigration : DataMigrationImpl {
        private readonly IContentManager _contentManager;
        private readonly IAutorouteService _autorouteService;
        private readonly ISessionFactoryHolder _sessionFactoryHolder;
        private readonly ShellSettings _shellSettings;

        public UpdateTo14DataMigration(
            IContentManager contentManager, 
            IAutorouteService autorouteService,
            ISessionFactoryHolder sessionFactoryHolder,
            ShellSettings shellSettings) {
            _contentManager = contentManager;
            _autorouteService = autorouteService;
            _sessionFactoryHolder = sessionFactoryHolder;
            _shellSettings = shellSettings;
        }

        public int Create() {
            return 1;

            var sessionFactory = _sessionFactoryHolder.GetSessionFactory();
            var session = sessionFactory.OpenSession();

            // migrating pages
            ContentDefinitionManager.AlterTypeDefinition("Page", 
                builder => builder
                    .WithPart("AutoroutePart")
                    .WithPart("TitlePart"));

            var pages = _contentManager.HqlQuery().ForType("Page").List();
            
            foreach(dynamic page in pages) {
                var autoroutePart = ((ContentItem)page).As<AutoroutePart>();
                var titlePart = ((ContentItem)page).As<TitlePart>();
                
                using (new TransactionScope(TransactionScopeOption.Suppress)) {
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
            }

            ContentDefinitionManager.AlterTypeDefinition("Page", builder => builder.RemovePart("RoutePart"));

            // migrating blogs
            ContentDefinitionManager.AlterTypeDefinition("Blog", builder => builder.WithPart("AutoroutePart").WithPart("TitlePart"));
            var blogs = _contentManager.HqlQuery().ForType("Blog").List();

            foreach (dynamic blog in blogs) {
                var autoroutePart = ((ContentItem)blog).As<AutoroutePart>();
                var titlePart = ((ContentItem)blog).As<TitlePart>();

                using (new TransactionScope(TransactionScopeOption.Suppress)) {
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
            }

            // migrating blog posts
            ContentDefinitionManager.AlterTypeDefinition("BlogPost", builder => builder.WithPart("AutoroutePart").WithPart("TitlePart"));
            var blogposts = _contentManager.HqlQuery().ForType("BlogPost").List();

            foreach (dynamic blogpost in blogposts) {
                var autoroutePart = ((ContentItem)blogpost).As<AutoroutePart>();
                var titlePart = ((ContentItem)blogpost).As<TitlePart>();

                using (new TransactionScope(TransactionScopeOption.Suppress)) {
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
            }

            // migrating containers/list
            // todo

            _autorouteService.CreatePattern("Page", "Title", "{Content.Slug}", "about-us", true);
            _autorouteService.CreatePattern("Blog", "Title", "{Content.Slug}", "my-blog", true);
            _autorouteService.CreatePattern("BlogPost", "Blog and Title", "{Content.Container.Path}/{Content.Slug}", "my-blog/a-blog-post", true);

            return 1;
        }

        private string GetPrefixedTableName(string tableName) {
            if(string.IsNullOrWhiteSpace(_shellSettings.DataTablePrefix)) {
                return tableName;
            }

            return _shellSettings.DataTablePrefix + "_" + tableName;
        }
    }
}