using System.Xml;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.ViewModels;
using Orchard.Localization;
using Orchard.Security;

namespace Orchard.Core.Common.Drivers {
    public class CommonPartDriver : ContentPartDriver<CommonPart> {
        private readonly IContentManager _contentManager;
        private readonly IMembershipService _membershipService;

        public CommonPartDriver(
            IOrchardServices services,
            IContentManager contentManager,
            IMembershipService membershipService) {
            _contentManager = contentManager;
            _membershipService = membershipService;
            T = NullLocalizer.Instance;
            Services = services;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        protected override string Prefix {
            get { return "CommonPart"; }
        }

        protected override DriverResult Display(CommonPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Common_Metadata",
                             () => shapeHelper.Parts_Common_Metadata()),
                ContentShape("Parts_Common_Metadata_Summary",
                             () => shapeHelper.Parts_Common_Metadata_Summary()),
                ContentShape("Parts_Common_Metadata_SummaryAdmin",
                             () => shapeHelper.Parts_Common_Metadata_SummaryAdmin())
                );
        }

        protected override DriverResult Editor(CommonPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(CommonPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new ContainerEditorViewModel();
            if (part.Container != null)
                model.ContainerId = part.Container.ContentItem.Id;

            if (updater != null) {
                var priorContainerId = model.ContainerId;
                updater.TryUpdateModel(model, Prefix, null, null);

                if (model.ContainerId != null && model.ContainerId != priorContainerId) {
                    var newContainer = _contentManager.Get((int)model.ContainerId, VersionOptions.Latest);
                    if (newContainer == null) {
                        updater.AddModelError("CommonPart.ContainerId", T("Invalid container"));
                    } else {
                        part.Container = newContainer;
                    }
                }
            }

            return ContentShape("Parts_Common_Container_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts.Common.Container", Model: model, Prefix: Prefix));
        }

        protected override void Importing(CommonPart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "Owner", owner => {
                var contentIdentity = new ContentIdentity(owner);

                // use the super user if the referenced one doesn't exist;
                part.Owner =
                    _membershipService.GetUser(contentIdentity.Get("User.UserName"))
                    ?? _membershipService.GetUser(Services.WorkContext.CurrentSite.SuperUser);
            });

            context.ImportAttribute(part.PartDefinition.Name, "Container", container =>
                part.Container = context.GetItemFromSession(container)
            );

            context.ImportAttribute(part.PartDefinition.Name, "CreatedUtc", createdUtc =>
                part.CreatedUtc = XmlConvert.ToDateTime(createdUtc, XmlDateTimeSerializationMode.Utc)
            );

            context.ImportAttribute(part.PartDefinition.Name, "PublishedUtc", publishedUtc =>
                part.PublishedUtc = XmlConvert.ToDateTime(publishedUtc, XmlDateTimeSerializationMode.Utc)
            );

            context.ImportAttribute(part.PartDefinition.Name, "ModifiedUtc", modifiedUtc =>
                part.ModifiedUtc = XmlConvert.ToDateTime(modifiedUtc, XmlDateTimeSerializationMode.Utc)
            );
        }

        protected override void Exporting(CommonPart part, ExportContentContext context) {
            if (part.Owner != null) {
                var ownerIdentity = _contentManager.GetItemMetadata(part.Owner).Identity;
                context.Element(part.PartDefinition.Name).SetAttributeValue("Owner", ownerIdentity.ToString());
            }

            if (part.Container != null) {
                var containerIdentity = _contentManager.GetItemMetadata(part.Container).Identity;
                context.Element(part.PartDefinition.Name).SetAttributeValue("Container", containerIdentity.ToString()); 
            }

            if (part.CreatedUtc != null) {
                context.Element(part.PartDefinition.Name)
                    .SetAttributeValue("CreatedUtc", XmlConvert.ToString(part.CreatedUtc.Value, XmlDateTimeSerializationMode.Utc));
            }
            if (part.PublishedUtc != null) {
                context.Element(part.PartDefinition.Name)
                    .SetAttributeValue("PublishedUtc", XmlConvert.ToString(part.PublishedUtc.Value, XmlDateTimeSerializationMode.Utc));
            }
            if (part.ModifiedUtc != null) {
                context.Element(part.PartDefinition.Name)
                    .SetAttributeValue("ModifiedUtc", XmlConvert.ToString(part.ModifiedUtc.Value, XmlDateTimeSerializationMode.Utc));
            }
        }
    }
}