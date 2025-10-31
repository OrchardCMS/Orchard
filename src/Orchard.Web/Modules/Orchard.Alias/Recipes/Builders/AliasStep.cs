using System.Linq;
using System.Xml.Linq;
using Orchard.Alias.Implementation.Holder;
using Orchard.Alias.Records;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Recipes.Services;

namespace Orchard.Alias.Recipes.Builders
{
    public class AliasStep : RecipeBuilderStep
    {
        private readonly IRepository<AliasRecord> _aliasRecordepository;
        private readonly IAliasHolder _aliasHolder;

        public AliasStep(IRepository<AliasRecord> aliasRecordRepository, IAliasHolder aliasHolder)
        {
            _aliasRecordepository = aliasRecordRepository;
            _aliasHolder = aliasHolder;
        }

        public override string Name => "Alias";

        public override LocalizedString DisplayName => T("Aliases");

        public override LocalizedString Description => T("Exports unmanaged aliases.");

        public override void Build(BuildContext context)
        {
            var aliases = _aliasHolder.GetMaps().SelectMany(m => m.GetAliases()).Where(m => m.IsManaged == false).OrderBy(m => m.Path).ToList();

            if (!aliases.Any())
                return;

            var root = new XElement("Aliases");
            context.RecipeDocument.Element("Orchard").Add(root);

            foreach (var alias in aliases)
            {
                var aliasElement = new XElement("Alias", new XAttribute("Path", alias.Path));

                var routeValuesElement = new XElement("RouteValues");
                foreach (var routeValue in alias.RouteValues)
                {
                    routeValuesElement.Add(new XElement("Add", new XAttribute("Key", routeValue.Key), new XAttribute("Value", routeValue.Value)));
                }

                aliasElement.Add(routeValuesElement);
                root.Add(aliasElement);
            }
        }
    }
}