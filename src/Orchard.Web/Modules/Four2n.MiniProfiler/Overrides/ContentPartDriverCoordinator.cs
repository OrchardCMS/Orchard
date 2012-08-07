using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Logging;
using Orchard.Environment.Extensions;
using Four2n.Orchard.MiniProfiler.Services;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement;
using Orchard;
using System;

namespace Four2n.MiniProfilter.Overrides {
    [OrchardSuppressDependency("Orchard.ContentManagement.Drivers.Coordinators.ContentPartDriverCoordinator")]
    public class ProfilingContentPartDriverCoordinator : ContentHandlerBase {
        private readonly IEnumerable<IContentPartDriver> _drivers;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IProfilerService _profiler;

        public ProfilingContentPartDriverCoordinator(IEnumerable<IContentPartDriver> drivers, IContentDefinitionManager contentDefinitionManager, IProfilerService profiler) {
            _drivers = drivers;
            _contentDefinitionManager = contentDefinitionManager;
            Logger = NullLogger.Instance;
            _profiler = profiler;
        }

        public ILogger Logger { get; set; }

        public override void Activating(ActivatingContentContext context) {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentType);
            if (contentTypeDefinition == null)
                return;

            var partInfos = _drivers.SelectMany(cpp => cpp.GetPartInfo());

            foreach (var typePartDefinition in contentTypeDefinition.Parts) {
                var partName = typePartDefinition.PartDefinition.Name;
                var partInfo = partInfos.FirstOrDefault(pi => pi.PartName == partName);
                var part = partInfo != null 
                    ? partInfo.Factory(typePartDefinition) 
                    : new ContentPart { TypePartDefinition = typePartDefinition };
                context.Builder.Weld(part);
            }
        }

        public override void GetContentItemMetadata(GetContentItemMetadataContext context) {
            _drivers.Invoke(driver => driver.GetContentItemMetadata(context), Logger);
        }

        public override void BuildDisplay(BuildDisplayContext context) {
            _drivers.Invoke(driver => {
                var key = "Driver:"+driver.GetType().FullName;
                _profiler.StepStart(key, String.Format("ContentPartDriver: {0}", driver.GetType().FullName));
                var result = driver.BuildDisplay(context);

                if (result != null) {
                    var key2 = "DriverApply:" + driver.GetType().FullName;
                    _profiler.StepStart(key2, String.Format("ApplyDriver", driver.GetType().FullName));
                    result.Apply(context);
                    _profiler.StepStop(key2);
                }

                _profiler.StepStop(key);
                
            }, Logger);
        }

        public override void BuildEditor(BuildEditorContext context) {
            _drivers.Invoke(driver => {
                var result = driver.BuildEditor(context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public override void UpdateEditor(UpdateEditorContext context) {
            _drivers.Invoke(driver => {
                var result = driver.UpdateEditor(context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public override void Importing(ImportContentContext context) {
            foreach (var contentPartDriver in _drivers) {
                contentPartDriver.Importing(context);
            }
        }

        public override void Imported(ImportContentContext context) {
            foreach (var contentPartDriver in _drivers) {
                contentPartDriver.Imported(context);
            }
        }

        public override void Exporting(ExportContentContext context) {
            foreach (var contentPartDriver in _drivers) {
                contentPartDriver.Exporting(context);
            }
        }

        public override void Exported(ExportContentContext context) {
            foreach (var contentPartDriver in _drivers) {
                contentPartDriver.Exported(context);
            }
        }
    }
}