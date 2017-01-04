using System.Collections.Generic;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Extensions;
using Orchard.Glimpse.Models;
using Orchard.Glimpse.Services;

namespace Orchard.Glimpse.AlternateImplementation {
    [OrchardFeature(FeatureNames.Parts)]
    public class GlimpseContentPartDriver : IDecorator<IContentPartDriver>, IContentPartDriver {
        private readonly IContentPartDriver _decoratedService;
        private readonly IGlimpseService _glimpseService;

        public GlimpseContentPartDriver(IContentPartDriver decoratedService, IGlimpseService glimpseService) {
            _decoratedService = decoratedService;
            _glimpseService = glimpseService;
        }

        public DriverResult BuildDisplay(BuildDisplayContext context) {
            var driverResult = _decoratedService.BuildDisplay(context);

            return driverResult == null ? null : new GlimpseDriverResult(driverResult, _glimpseService);
        }

        public DriverResult BuildEditor(BuildEditorContext context) {
            return _decoratedService.BuildEditor(context);
        }

        public void Exported(ExportContentContext context) {
            _decoratedService.Exported(context);
        }

        public void Cloning(CloneContentContext context) {
            _decoratedService.Cloning(context);
        }

        public void Cloned(CloneContentContext context) {
            _decoratedService.Cloned(context);
        }

        public void Exporting(ExportContentContext context) {
            _decoratedService.Exporting(context);
        }

        public void GetContentItemMetadata(GetContentItemMetadataContext context) {
            _decoratedService.GetContentItemMetadata(context);
        }

        public IEnumerable<ContentPartInfo> GetPartInfo() {
            return _decoratedService.GetPartInfo();
        }

        public void ImportCompleted(ImportContentContext context) {
            _decoratedService.ImportCompleted(context);
        }

        public void Imported(ImportContentContext context) {
            _decoratedService.Imported(context);
        }

        public void Importing(ImportContentContext context) {
            _decoratedService.Importing(context);
        }

        public DriverResult UpdateEditor(UpdateEditorContext context) {
            return _decoratedService.UpdateEditor(context);
        }
    }
}