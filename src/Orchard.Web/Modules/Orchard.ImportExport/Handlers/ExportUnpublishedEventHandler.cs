using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.Localization;

namespace Orchard.ImportExport.Handlers {
    [OrchardFeature("Orchard.Deployment")]
    public class ExportUnpublishedEventHandler : IExportEventHandler, ICustomExportStep {
        public const string StepName = "ExportUnpublishedContent";

        private readonly IContentManager _contentManager;

        public ExportUnpublishedEventHandler(IContentManager contentManager) {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Register(IList<string> steps) {
            steps.Add(StepName);
        }

        public void Exporting(ExportContext context) {
            //Not required
        }

        public void Exported(ExportContext context) {
            var unpublishedDataStep = context.ExportOptions.CustomSteps
                .FirstOrDefault(step => step.StartsWith(StepName));

            if (unpublishedDataStep == null) return;

            DateTime? unpublishedFrom = null;

            //May include a start date in the form of UnpublishedData:2013-01-01
            if (unpublishedDataStep.IndexOf(':') > 0) {
                var unpublishedFromStr = unpublishedDataStep.Substring(unpublishedDataStep.IndexOf(':')).TrimStart(':');

                DateTime tempDate;
                if (DateTime.TryParse(unpublishedFromStr, out tempDate)) {
                    unpublishedFrom = tempDate.ToUniversalTime();
                }
            }

            var unpublishedElement = ExportUnpublishedData(unpublishedFrom, context.ContentTypes);

            if (!unpublishedElement.Elements().Any()) return;
            
            var orchardElement = context.Document.Element("Orchard");
            if (orchardElement == null) {
                throw new InvalidOperationException(T("Recipe has no Orchard top-level element.").Text);
            }
            orchardElement.Add(ExportUnpublishedData(unpublishedFrom, context.ContentTypes));
        }

        private XElement ExportUnpublishedData(DateTime? unpublishedFromUtc, IEnumerable<string> contentTypes) {
            var unpublishedData = new XElement(StepName);

            foreach (var contentItem in GetUnpublishedContentItems(unpublishedFromUtc, contentTypes)) {
                var element = new XElement(contentItem.ContentType);
                element.SetAttributeValue("Id", _contentManager.GetItemMetadata(contentItem).Identity.ToString());

                unpublishedData.Add(element);
            }

            return unpublishedData;
        }

        private IEnumerable<ContentItem> GetUnpublishedContentItems(DateTime? unpublishedFromUtc, IEnumerable<string> contentTypes) {
            Action<IHqlExpressionFactory> gtPredicate = exp => exp.Gt("UnpublishedUtc", unpublishedFromUtc);
            Action<IHqlExpressionFactory> notNullPredicate = exp => exp.IsNotNull("UnpublishedUtc");
            var predicate = unpublishedFromUtc.HasValue ? gtPredicate : notNullPredicate;

            return _contentManager.HqlQuery()
                .ForType(contentTypes.ToArray())
                .ForPart<DeployablePart>()
                .ForVersion(VersionOptions.AllVersions)
                .Where(a => a.ContentPartRecord(typeof (DeployablePartRecord)), predicate)
                .Where(a => a.ContentPartRecord(typeof (DeployablePartRecord)), exp => exp.Eq("Latest", true))
                //Needed to fix bug with HQL in orchard 1.7 pre-release where query would fail if order not specified
                .OrderBy(a => a.ContentItem(), o => o.Asc("Id"))
                .List()
                .Select(c => c.ContentItem)
                .ToList();
        }
    }
}
