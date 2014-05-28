using System.Linq;
using Orchard.AuditTrail.Models;
using Orchard.AuditTrail.Services;
using Orchard.AuditTrail.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Security;

namespace Orchard.AuditTrail.Drivers {
    public class AuditTrailSiteSettingsPartDriver : ContentPartDriver<AuditTrailSiteSettingsPart> {
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IAuthorizer _authorizer;

        public AuditTrailSiteSettingsPartDriver(IAuditTrailManager auditTrailManager, IAuthorizer authorizer) {
            _auditTrailManager = auditTrailManager;
            _authorizer = authorizer;
        }

        protected override DriverResult Editor(AuditTrailSiteSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(AuditTrailSiteSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!_authorizer.Authorize(Permissions.ManageAuditTrailSettings))
                return null;

            return ContentShape("Parts_AuditTrailSiteSettings_Edit", () => {
                var descriptors = _auditTrailManager.Describe();
                var eventSettings = part.EventSettings.ToList();
                var viewModel = new AuditTrailSiteSettingsViewModel {
                    AutoTrim = part.AutoTrim,
                    AutoTrimThreshold = part.AutoTrimThreshold,
                    Categories = (from categoryDescriptor in descriptors
                        select new AuditTrailCategorySettingsViewModel {
                            Category = categoryDescriptor.Category,
                            Name = categoryDescriptor.Name,
                            Events = (from eventDescriptor in categoryDescriptor.Events
                                let eventSetting = eventSettings.FirstOrDefault(x => x.EventName == eventDescriptor.Event)
                                select new AuditTrailEventSettingsViewModel {
                                    Event = eventDescriptor.Event,
                                    Name = eventDescriptor.Name,
                                    Description = eventDescriptor.Description,
                                    IsEnabled = eventSetting != null ? eventSetting.IsEnabled : eventDescriptor.IsEnabledByDefault
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

                            eventSetting.IsEnabled = eventSettingViewModel.IsEnabled;
                        }
                        part.EventSettings = eventSettings;
                        part.AutoTrim = viewModel.AutoTrim;
                        part.AutoTrimThreshold = viewModel.AutoTrimThreshold;
                    }
                }

                return shapeHelper.EditorTemplate(TemplateName: "Parts.AuditTrailSiteSettings", Model: viewModel, Prefix: Prefix);
            }).OnGroup("Audit Trail");
        }
    }
}