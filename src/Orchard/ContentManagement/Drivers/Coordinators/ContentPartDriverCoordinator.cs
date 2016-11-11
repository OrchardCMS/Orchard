using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Logging;

namespace Orchard.ContentManagement.Drivers.Coordinators {
    /// <summary>
    /// This component coordinates how parts are taking part in the rendering when some content needs to be rendered.
    /// It will dispatch BuildDisplay/BuildEditor to all <see cref="IContentPartDriver"/> implementations.
    /// </summary>
    public class ContentPartDriverCoordinator : ContentHandlerBase {
        private readonly IEnumerable<IContentPartDriver> _drivers;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentPartDriverCoordinator(IEnumerable<IContentPartDriver> drivers, IContentDefinitionManager contentDefinitionManager) {
            _drivers = drivers;
            _contentDefinitionManager = contentDefinitionManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public override void Activating(ActivatingContentContext context) {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentType);
            if (contentTypeDefinition == null)
                return;

            var partInfos = _drivers.SelectMany(cpp => cpp.GetPartInfo()).ToList();

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
                var result = driver.BuildDisplay(context);
                if (result != null)
                    result.Apply(context);
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

        public override void ImportCompleted(ImportContentContext context) {
            foreach (var contentPartDriver in _drivers) {
                contentPartDriver.ImportCompleted(context);
            }
        }

        public override void Exporting(ExportContentContext context) {
            foreach (var contentPartDriver in _drivers.OrderBy(x => x.GetPartInfo().First().PartName)) {
                contentPartDriver.Exporting(context);
            }
        }

        public override void Exported(ExportContentContext context) {
            foreach (var contentPartDriver in _drivers.OrderBy(x => x.GetPartInfo().First().PartName)) {
                contentPartDriver.Exported(context);
            }
        }

        public override void Cloning(CloneContentContext context) {
            var dGroups = _drivers.GroupBy(cpd => cpd.GetPartInfo().FirstOrDefault().PartName);
            Type contentPartDriverType = typeof(ContentPartDriver<>);
            foreach (var driverGroup in dGroups) {
                //if no driver implements Cloning, run the fallback for the part
                //otherwise, invoke Cloning for all these drivers.

                //get baseType of driver (this is ContentPartDriver<TContent>)
                Type baseDriverType = driverGroup.First().GetType().BaseType;
                //find the definition of the virtual Cloning method (we know it's there because we put it in the base abstract class)
                var cloningMethodInfo = baseDriverType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(mi => mi.Name == "Cloning").FirstOrDefault();

                bool noCloningImplementation = true;
                foreach (var contentPartDriver in driverGroup) {
                    //if we find an implementation of cloning, break
                    if (contentPartDriver.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Where(mi => mi.Name == "Cloning").FirstOrDefault() != null) {
                        noCloningImplementation = false;
                        break;
                    }
                }
                if (noCloningImplementation) {
                    //fallback
                    //invoke a private method we defined in ContentPartDriver. The CloneFallBack method is not in the IContentPartDriver interface and private to avoid overrides
                    //baseDriverType.GetMethod("CloneFallBack", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(driverGroup.First(), new object[] { context });
                }
                else {
                    foreach (var contentPartDriver in driverGroup) {
                        contentPartDriver.Cloning(context);
                    }
                }
            }
            //foreach (var contentPartDriver in _drivers) {
            //    contentPartDriver.Cloning(context);
            //}
        }

        public override void Cloned(CloneContentContext context) {
            foreach (var contentPartDriver in _drivers) {
                contentPartDriver.Cloned(context);
            }
        }
    }
}