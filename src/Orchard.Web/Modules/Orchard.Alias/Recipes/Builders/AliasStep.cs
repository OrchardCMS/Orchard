using System.Linq;
using System.Xml.Linq;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Recipes.Services;
using Orchard.Alias.Records;
using Orchard.Alias.Implementation.Holder;

namespace Orchard.Roles.Recipes.Builders {
    public class RolesStep : RecipeBuilderStep {
        private readonly IRepository<AliasRecord> _aliasRecordepository;
        private readonly IAliasHolder _aliasHolder;

        public RolesStep(IRepository<AliasRecord> aliasRecordRepository, IAliasHolder aliasHolder)
        {
            _aliasRecordepository = aliasRecordRepository;
            _aliasHolder = aliasHolder;
        }

        public override string Name {
            get { return "Alias"; }
        }

        public override LocalizedString DisplayName {
            get { return T("Alias"); }
        }

        public override LocalizedString Description {
            get { return T("Exports the aliases."); }
        }

        public override void Build(BuildContext context) {
            //var aliases = _aliasRecordepository.Table.ToList();

            var aliases = _aliasHolder.GetMaps().SelectMany(m => m.GetAliases()).Where(m => m.IsManaged== false).ToList();

            if (!aliases.Any())
                return;

            var root = new XElement("Aliases");
            context.RecipeDocument.Element("Orchard").Add(root);

            foreach (var alias in aliases.OrderBy(x => x.Path))
            {
                var aliasElement = new XElement("Alias", new XAttribute("Path", alias.Path));
                //root.Add(
                //    new XElement("Alias",
                //        new XAttribute("Path", alias.Path)));
                var routeValuesElement = new XElement("RouteValues");
                foreach (var routeValue in alias.RouteValues)
                {
                    routeValuesElement.Add(new XElement("Add", new XAttribute("Key", routeValue.Key), new XAttribute("Value", routeValue.Value)));
                }

                aliasElement.Add(routeValuesElement);
                root.Add(aliasElement);
            }

            //add a collection of all the alias paths in this site so that the importing site can remove aliases that don't exist remotely
            //var pathsElement = new XElement("Paths");
            //foreach (var aliasInfo in aliases)
            //{
            //    pathsElement.Add(new XElement("Add", new XAttribute("Path", aliasInfo.Path)));
            //}

            //root.Add(pathsElement);

            //var rootElement = context.Document.Descendants("Orchard").FirstOrDefault();

            //if (rootElement == null)
            //{
            //    var ex = new OrchardException(T("Could not export this site's Aliases because the document passed via the Export Context did not contain a node called 'Orchard'. The document was malformed."));
            //    Logger.Error(ex, ex.Message);
            //    throw ex;
            //}

            //rootElement.Add(xmlElement);
        }

        //public void Exporting(ExportContext context)
        //{
        //    if (!context.ExportOptions.CustomSteps.Contains("Aliases")) { return; }

        //    var xmlElement = new XElement("Aliases");

        //    var autoroutePaths = _contentManager.Query<AutoroutePart>().List().Select(p => p.Path);
        //    var allAliasInfos = _aliasHolder.GetMaps().SelectMany(m => m.GetAliases()).ToList();

        //    //we need to remove any aliases that are autoroutes because the remote conent id may not sync up with the local content id. the autoroutes will be imported as part of the content import
        //    var aliasInfosToExport = allAliasInfos.Where(ai => !autoroutePaths.Contains(ai.Path));

        //    foreach (var aliasInfo in aliasInfosToExport)
        //    {
        //        var aliasElement = new XElement("Alias", new XAttribute("Path", aliasInfo.Path));

        //        var routeValuesElement = new XElement("RouteValues");
        //        foreach (var routeValue in aliasInfo.RouteValues)
        //        {
        //            routeValuesElement.Add(new XElement("Add", new XAttribute("Key", routeValue.Key), new XAttribute("Value", routeValue.Value)));
        //        }

        //        aliasElement.Add(routeValuesElement);
        //        xmlElement.Add(aliasElement);
        //    }

        //    //add a collection of all the alias paths in this site so that the importing site can remove aliases that don't exist remotely
        //    var pathsElement = new XElement("Paths");
        //    foreach (var aliasInfo in allAliasInfos)
        //    {
        //        pathsElement.Add(new XElement("Add", new XAttribute("Path", aliasInfo.Path)));
        //    }

        //    xmlElement.Add(pathsElement);

        //    var rootElement = context.Document.Descendants("Orchard").FirstOrDefault();

        //    if (rootElement == null)
        //    {
        //        var ex = new OrchardException(T("Could not export this site's Aliases because the document passed via the Export Context did not contain a node called 'Orchard'. The document was malformed."));
        //        Logger.Error(ex, ex.Message);
        //        throw ex;
        //    }

        //    rootElement.Add(xmlElement);
        //}


    }
}