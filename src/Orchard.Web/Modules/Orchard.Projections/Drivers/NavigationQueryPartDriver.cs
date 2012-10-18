using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Projections.Models;
using Orchard.Projections.ViewModels;

namespace Orchard.Projections.Drivers {
    public class NavigationQueryPartDriver : ContentPartDriver<NavigationQueryPart> {
        private readonly IRepository<QueryPartRecord> _queryRepository;
        private const string TemplateName = "Parts/NavigationQueryPart";

        public NavigationQueryPartDriver(
            IOrchardServices services,
            IRepository<QueryPartRecord> queryRepository) {
            _queryRepository = queryRepository;
            T = NullLocalizer.Instance;
            Services = services;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        protected override string Prefix { get { return "NavigationQueryPart"; } }

        protected override DriverResult Editor(NavigationQueryPart part, dynamic shapeHelper) {
            return ContentShape("Parts_NavigationQueryPart_Edit", () => {

                var model = new NavigationQueryPartEditViewModel {
                    Items = part.Items,
                    Skip = part.Skip,
                    QueryRecordId = part.QueryPartRecord == null ? "-1" :　part.QueryPartRecord.Id.ToString(), 
                    Queries = Services.ContentManager.Query<QueryPart>().Join<TitlePartRecord>().OrderBy(x => x.Title).List(),
                };

                return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix);
            });
        }

        protected override DriverResult Editor(NavigationQueryPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new NavigationQueryPartEditViewModel();

            if (updater.TryUpdateModel(model, Prefix, null, null)) {
                part.Record.Items = model.Items;
                part.Record.Skip = model.Skip;
                part.Record.QueryPartRecord = _queryRepository.Get(Int32.Parse(model.QueryRecordId));
            }

            return Editor(part, shapeHelper);
        }

        protected override void Importing(NavigationQueryPart part, ImportContentContext context) {
            IfNotNull(context.Attribute(part.PartDefinition.Name, "Items"), x => part.Record.Items = Int32.Parse(x));
            IfNotNull(context.Attribute(part.PartDefinition.Name, "Offset"), x => part.Record.Skip = Int32.Parse(x));
        }

        protected override void Imported(NavigationQueryPart part, ImportContentContext context) {
            // assign the query only when everything is imported
            var query = context.Attribute(part.PartDefinition.Name, "Query");
            if (query != null) {
                part.Record.QueryPartRecord = context.GetItemFromSession(query).As<QueryPart>().Record;
            }
        }

        private static void IfNotNull<T>(T value, Action<T> then) where T : class {
            if(value != null) {
                then(value);
            }
        }

        protected override void Exporting(NavigationQueryPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Items", part.Record.Items);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Offset", part.Record.Skip);

            if (part.Record.QueryPartRecord != null) {
                var queryPart = Services.ContentManager.Query<QueryPart, QueryPartRecord>("Query").Where(x => x.Id == part.Record.QueryPartRecord.Id).List().FirstOrDefault();
                if (queryPart != null) {
                    var queryIdentity = Services.ContentManager.GetItemMetadata(queryPart).Identity;
                    context.Element(part.PartDefinition.Name).SetAttributeValue("Query", queryIdentity.ToString());
                }
            }
        }
    }
}
