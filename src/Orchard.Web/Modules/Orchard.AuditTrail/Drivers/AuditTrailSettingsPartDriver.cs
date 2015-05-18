using System.Collections.Generic;
using System.Linq;
using Orchard.AuditTrail.Models;
using Orchard.AuditTrail.Services;
using Orchard.AuditTrail.Services.Models;
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
                var categoriesQuery =
                    from categoryDescriptor in descriptors
                    let eventsQuery =
                        from eventDescriptor in categoryDescriptor.Events
                        let eventSetting = GetOrCreate(eventSettings, eventDescriptor)
                        select new AuditTrailEventSettingsViewModel {
                            Event = eventDescriptor.Event,
                            Name = eventDescriptor.Name,
                            Description = eventDescriptor.Description,
                            IsEnabled = eventDescriptor.IsMandatory || eventSetting.IsEnabled,
                            IsMandatory = eventDescriptor.IsMandatory
                        }
                    select new AuditTrailCategorySettingsViewModel {
                        Category = categoryDescriptor.Category,
                        Name = categoryDescriptor.Name,
                        Events = eventsQuery.ToList()
                    };

                var viewModel = new AuditTrailSettingsViewModel {
                    Categories = categoriesQuery.ToList(),
                    EnableClientIpAddressLogging = part.EnableClientIpAddressLogging
                };

                // Update the settings as we may have added new settings.
                part.EventSettings = eventSettings;

                if (updater != null) {
                    var eventsDictionary = _auditTrailManager.DescribeProviders().Describe().SelectMany(x => x.Events).ToDictionary(x => x.Event);
                    if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                        foreach (var eventSettingViewModel in viewModel.Categories.SelectMany(x => x.Events)) {
                            var eventSetting = eventSettings.First(x => x.EventName == eventSettingViewModel.Event);
                            var descriptor = eventsDictionary[eventSetting.EventName];

                            eventSetting.IsEnabled = eventSettingViewModel.IsEnabled || descriptor.IsMandatory;
                        }
                        part.EventSettings = eventSettings;
                        part.EnableClientIpAddressLogging = viewModel.EnableClientIpAddressLogging;
                    }
                }

                return shapeHelper.EditorTemplate(TemplateName: "Parts.AuditTrailSettings", Model: viewModel, Prefix: Prefix);
            }).OnGroup("Audit Trail");
        }

        /// <summary>
        /// We're creating settings on the fly so that when the user updates the settings the first time, we won't log a massive amount of event settings that have changed.
        /// </summary>
        private AuditTrailEventSetting GetOrCreate(ICollection<AuditTrailEventSetting> settings, AuditTrailEventDescriptor descriptor) {
            var setting = settings.FirstOrDefault(x => x.EventName == descriptor.Event);

            if (setting == null) {
                setting = new AuditTrailEventSetting {
                    EventName = descriptor.Event,
                    IsEnabled = descriptor.IsMandatory || descriptor.IsEnabledByDefault
                };

                settings.Add(setting);
            }

            return setting;
        }
    }
}