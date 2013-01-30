using System;
using System.Collections.Generic;
using Contrib.Taxonomies.Routing;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Drivers;
using Contrib.Taxonomies.Models;
using Contrib.Taxonomies.Services;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Contrib.Taxonomies.Drivers {
    public class TermPartDriver : ContentPartDriver<TermPart> {

        private readonly ITaxonomyService _taxonomyService;
        private readonly IContentManager _contentManager;
        private readonly ITermPathConstraint _termPathConstraint;

        public TermPartDriver(
            ITaxonomyService taxonomyService,
            IContentManager contentManager,
            ITermPathConstraint termPathConstraint ) {
            _taxonomyService = taxonomyService;
            _contentManager = contentManager;
            _termPathConstraint = termPathConstraint;
            T = NullLocalizer.Instance;
        }

        Localizer T { get; set; }

        protected override string Prefix { get { return "Term"; } }

        protected override DriverResult Editor(TermPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Taxonomies_Term_Fields",
                    () => shapeHelper.EditorTemplate(TemplateName: "Parts/Taxonomies.Term.Fields", Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(TermPart termPart, IUpdateModel updater, dynamic shapeHelper) {
            if (updater.TryUpdateModel(termPart, Prefix, null, null)) {
                var existing = _taxonomyService.GetTermByName(termPart.TaxonomyId, termPart.Name);
                if (existing != null && existing.Record != termPart.Record && existing.Container.ContentItem.Record == termPart.Container.ContentItem.Record) {
                    updater.AddModelError("Name", T("The term {0} already exists at this level", termPart.Name));
                }
            }

            return Editor(termPart, shapeHelper);
        }

        protected override void Exporting(TermPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Count", part.Record.Count);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Selectable", part.Record.Selectable);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Weight", part.Record.Weight);

            var taxonomy = _contentManager.Get(part.Record.TaxonomyId);
            var identity = _contentManager.GetItemMetadata(taxonomy).Identity.ToString();
            context.Element(part.PartDefinition.Name).SetAttributeValue("TaxonomyId", identity);

            var identityPaths = new List<string>();
            foreach(string pathPart in part.Record.Path.Split('/')) {
                if(String.IsNullOrEmpty(pathPart)) {
                    continue;
                }

                var parent = _contentManager.Get(Int32.Parse(pathPart));
                identityPaths.Add(_contentManager.GetItemMetadata(parent).Identity.ToString());
            }

            context.Element(part.PartDefinition.Name).SetAttributeValue("Path", String.Join(",", identityPaths.ToArray()));

        }

        protected override void Importing(TermPart part, ImportContentContext context) {
            part.Record.Count = Int32.Parse(context.Attribute(part.PartDefinition.Name, "Count"));
            part.Record.Selectable = Boolean.Parse(context.Attribute(part.PartDefinition.Name, "Selectable"));
            part.Record.Weight = Int32.Parse(context.Attribute(part.PartDefinition.Name, "Weight"));

            var identity = context.Attribute(part.PartDefinition.Name, "TaxonomyId");
            var contentItem = context.GetItemFromSession(identity);
            
            if (contentItem == null) {
                throw new OrchardException(T("Unknown taxonomy: {0}", identity));
            } 
            
            part.Record.TaxonomyId = contentItem.Id;


            part.Record.Path = "/";
            foreach(var identityPath in context.Attribute(part.PartDefinition.Name, "Path").Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries)) {
                var pathContentItem = context.GetItemFromSession(identityPath);
                part.Record.Path += pathContentItem.Id + "/";
            }
        }
    }
}