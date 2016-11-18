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

        public ContentPartDriverCoordinator(IEnumerable<IContentPartCloningDriver> drivers, IContentDefinitionManager contentDefinitionManager) {
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
            foreach (var driverGroup in dGroups) {
                //if no driver implements Cloning, run the fallback for the part
                //otherwise, invoke Cloning for all these drivers.

                bool noCloningImplementation = true;
                foreach (var contentPartDriver in driverGroup.Where(cpd => cpd is IContentPartCloningDriver)) {
                    //if we find an implementation of cloning, break
                    if (contentPartDriver.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Where(mi => mi.Name == "Cloning").FirstOrDefault() != null) {
                        noCloningImplementation = false;
                        break;
                    }
                }
                if (noCloningImplementation) {
                    //fallback
                    var ecc = new ExportContentContext(context.ContentItem, new System.Xml.Linq.XElement(System.Xml.XmlConvert.EncodeLocalName(context.ContentItem.ContentType)));
                    foreach (var contentPartDriver in driverGroup) {
                        contentPartDriver.Exporting(ecc);
                    }
                    foreach (var contentPartDriver in driverGroup) {
                        contentPartDriver.Exported(ecc);
                    }
                    var importContentSession = new ImportContentSession(context.ContentManager);
                    var copyId = context.CloneContentItem.Id.ToString();
                    importContentSession.Set(copyId, ecc.Data.Name.LocalName);
                    var icc = new ImportContentContext(context.CloneContentItem, ecc.Data, importContentSession);
                    foreach (var contentPartDriver in driverGroup) {
                        contentPartDriver.Importing(icc);
                    }
                    foreach (var contentPartDriver in driverGroup) {
                        contentPartDriver.Imported(icc);
                    }
                    foreach (var contentPartDriver in driverGroup) {
                        contentPartDriver.ImportCompleted(icc);
                    }
                }
                else {
                    foreach (var contentPartDriver in driverGroup.Select(cpd => cpd as IContentPartCloningDriver).Where(cpd => cpd != null)) {
                        contentPartDriver.Cloning(context);
                    }
                }
            }
        }

        public override void Cloned(CloneContentContext context) {
            foreach (var contentPartDriver in _drivers.Select(cpd => cpd as IContentPartCloningDriver).Where(cpd => cpd != null)) {
                contentPartDriver.Cloned(context);
            }
        }
    }
}