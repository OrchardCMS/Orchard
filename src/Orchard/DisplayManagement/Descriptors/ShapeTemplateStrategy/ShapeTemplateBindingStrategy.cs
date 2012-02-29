using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Orchard.Caching;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.VirtualPath;
using Orchard.Logging;
using Orchard.Utility.Extensions;

namespace Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy {
    public class ShapeTemplateBindingStrategy : IShapeTableProvider {
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IExtensionManager _extensionManager;
        private readonly ICacheManager _cacheManager;
        private readonly IVirtualPathMonitor _virtualPathMonitor;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IEnumerable<IShapeTemplateHarvester> _harvesters;
        private readonly IEnumerable<IShapeTemplateViewEngine> _shapeTemplateViewEngines;
        private readonly IParallelCacheContext _parallelCacheContext;


        public ShapeTemplateBindingStrategy(
            IEnumerable<IShapeTemplateHarvester> harvesters,
            ShellDescriptor shellDescriptor,
            IExtensionManager extensionManager,
            ICacheManager cacheManager,
            IVirtualPathMonitor virtualPathMonitor,
            IVirtualPathProvider virtualPathProvider,
            IEnumerable<IShapeTemplateViewEngine> shapeTemplateViewEngines,
            IParallelCacheContext parallelCacheContext) {

            _harvesters = harvesters;
            _shellDescriptor = shellDescriptor;
            _extensionManager = extensionManager;
            _cacheManager = cacheManager;
            _virtualPathMonitor = virtualPathMonitor;
            _virtualPathProvider = virtualPathProvider;
            _shapeTemplateViewEngines = shapeTemplateViewEngines;
            _parallelCacheContext = parallelCacheContext;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        private static IEnumerable<ExtensionDescriptor> Once(IEnumerable<FeatureDescriptor> featureDescriptors) {
            var once = new ConcurrentDictionary<string, object>();
            return featureDescriptors.Select(fd => fd.Extension).Where(ed => once.TryAdd(ed.Id, null)).ToList();
        }

        public void Discover(ShapeTableBuilder builder) {
            Logger.Information("Start discovering shapes");

            var harvesterInfos = _harvesters.Select(harvester => new { harvester, subPaths = harvester.SubPaths() });

            var availableFeatures = _extensionManager.AvailableFeatures();
            var activeFeatures = availableFeatures.Where(FeatureIsEnabled);
            var activeExtensions = Once(activeFeatures);

            var hits = _parallelCacheContext.RunInParallel(activeExtensions, extensionDescriptor => {
                Logger.Information("Start discovering candidate views filenames");
                var pathContexts = harvesterInfos.SelectMany(harvesterInfo => harvesterInfo.subPaths.Select(subPath => {
                    var basePath = Path.Combine(extensionDescriptor.Location, extensionDescriptor.Id).Replace(Path.DirectorySeparatorChar, '/');
                    var virtualPath = Path.Combine(basePath, subPath).Replace(Path.DirectorySeparatorChar, '/');
                    var fileNames = _cacheManager.Get(virtualPath, ctx => {
                        ctx.Monitor(_virtualPathMonitor.WhenPathChanges(virtualPath));
                        return _virtualPathProvider.ListFiles(virtualPath).Select(Path.GetFileName).ToReadOnlyCollection();
                    });
                    return new { harvesterInfo.harvester, basePath, subPath, virtualPath, fileNames };
                })).ToList();
                Logger.Information("Done discovering candidate views filenames");

                var fileContexts = pathContexts.SelectMany(pathContext => _shapeTemplateViewEngines.SelectMany(ve => {
                    var fileNames = ve.DetectTemplateFileNames(pathContext.fileNames);
                    return fileNames.Select(
                        fileName => new {
                            fileName = Path.GetFileNameWithoutExtension(fileName),
                            fileVirtualPath = Path.Combine(pathContext.virtualPath, fileName).Replace(Path.DirectorySeparatorChar, '/'),
                            pathContext
                        });
                }));

                var shapeContexts = fileContexts.SelectMany(fileContext => {
                    var harvestShapeInfo = new HarvestShapeInfo {
                        SubPath = fileContext.pathContext.subPath,
                        FileName = fileContext.fileName,
                        TemplateVirtualPath = fileContext.fileVirtualPath
                    };
                    var harvestShapeHits = fileContext.pathContext.harvester.HarvestShape(harvestShapeInfo);
                    return harvestShapeHits.Select(harvestShapeHit => new { harvestShapeInfo, harvestShapeHit, fileContext });
                });

                return shapeContexts.Select(shapeContext => new { extensionDescriptor, shapeContext }).ToList();
            }).SelectMany(hits2 => hits2);


            foreach (var iter in hits) {
                // templates are always associated with the namesake feature of module or theme
                var hit = iter;
                var featureDescriptors = iter.extensionDescriptor.Features.Where(fd => fd.Id == hit.extensionDescriptor.Id);
                foreach (var featureDescriptor in featureDescriptors) {                    
                    Logger.Debug("Binding {0} as shape [{1}] for feature {2}", 
                        hit.shapeContext.harvestShapeInfo.TemplateVirtualPath,
                        iter.shapeContext.harvestShapeHit.ShapeType,
                        featureDescriptor.Id);

                    builder.Describe(iter.shapeContext.harvestShapeHit.ShapeType)
                        .From(new Feature { Descriptor = featureDescriptor })
                        .BoundAs(
                            hit.shapeContext.harvestShapeInfo.TemplateVirtualPath,
                            shapeDescriptor => displayContext => Render(shapeDescriptor, displayContext, hit.shapeContext.harvestShapeInfo, hit.shapeContext.harvestShapeHit));
                }
            }

            Logger.Information("Done discovering shapes");
        }

        private bool FeatureIsEnabled(FeatureDescriptor fd) {
            return (DefaultExtensionTypes.IsTheme(fd.Extension.ExtensionType) && (fd.Id == "TheAdmin" || fd.Id == "SafeMode")) ||
                _shellDescriptor.Features.Any(sf => sf.Name == fd.Id);
        }

        private IHtmlString Render(ShapeDescriptor shapeDescriptor, DisplayContext displayContext, HarvestShapeInfo harvestShapeInfo, HarvestShapeHit harvestShapeHit) {
            Logger.Information("Rendering template file '{0}'", harvestShapeInfo.TemplateVirtualPath);

            var htmlHelper = new HtmlHelper(displayContext.ViewContext, displayContext.ViewDataContainer);
            var result = htmlHelper.Partial(harvestShapeInfo.TemplateVirtualPath, displayContext.Value);

            Logger.Information("Done rendering template file '{0}'", harvestShapeInfo.TemplateVirtualPath);
            return result;
        }

    }
}
