using System.Linq;
using Orchard.AuditTrail.Models;
using Orchard.AuditTrail.Services;
using Orchard.AuditTrail.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Security;

namespace Orchard.AuditTrail.Drivers {
    public class AuditTrailSettingsPartDriver : ContentPartDriver<AuditTrailSettingsPart> {
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IAuthorizer _authorizer;

        public AuditTrailSettingsPartDriver(IAuditTrailManager auditTrailManager, IAuthorizer authorizer) {
            _auditTrailManager = auditTrailManager;
            _authorizer = authorizer;
        }

        protected override DriverResult Editor(AuditTrailSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(AuditTrailSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!_authorizer.Authorize(Permissions.ManageAuditTrailSettings))
                return null;

            return ContentShape("Parts_AuditTrailSettings_Edit", () => {
                var descriptors = _auditTrailManager.DescribeCategories();
                var eventSettings = part.EventSettings.ToList();
                var viewModel = new AuditTrailSettingsViewModel {
                    Categories = (from categoryDescriptor in descriptors
                        select new AuditTrailCategorySettingsViewModel {
                            Category = categoryDescriptor.Category,
                            Name = categoryDescriptor.Name,
                            Events = 
                                (from eventDescriptor in categoryDescriptor.Events
                                let eventSetting = eventSettings.FirstOrDefault(x => x.EventName == eventDescriptor.Event)
                                select new AuditTrailEventSettingsViewModel {
                                    Event = eventDescriptor.Event,
                                    Name = eventDescriptor.Name,
                                    Description = eventDescriptor.Description,
                                    IsEnabled = eventDescriptor.IsMandatory || (eventSetting != null ? eventSetting.IsEnabled : eventDescriptor.IsEnabledByDefault),
                                    IsMandatory = eventDescriptor.IsMandatory
                                }).ToList()
                        }).ToList()
                };

                if (updater != null) {
                    if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                        foreach (var eventSettingViewModel in viewModel.Categories.SelectMany(x => x.Events)) {
                            var eventSetting = eventSettings.FirstOrDefault(x => x.EventName == eventSettingViewModel.Event);

                            if (eventSetting == null) {
                                eventSetting = new AuditTrailEventSetting { EventName = eventSettingViewModel.Event};
                                eventSettings.Add(eventSetting);
                            }

                            eventSetting.IsEnabled = eventSettingViewModel.IsEnabled || eventSettingViewModel.IsMandatory;
                        }
                        part.EventSettings = eventSettings;
                    }
                }

                return shapeHelper.EditorTemplate(TemplateName: "Parts.AuditTrailSettings", Model: viewModel, Prefix: Prefix);
            }).OnGroup("Audit Trail");
        }
    }
}