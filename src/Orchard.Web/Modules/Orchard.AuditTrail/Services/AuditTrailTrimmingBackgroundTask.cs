using System;
using System.Linq;
using System.Threading;
using Orchard.AuditTrail.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Services;
using Orchard.Settings;
using Orchard.Tasks;

namespace Orchard.AuditTrail.Services {
    [OrchardFeature("Orchard.AuditTrail.Trimming")]
    public class AuditTrailTrimmingBackgroundTask : Component, IBackgroundTask {
        private static readonly object _sweepLock = new object();
        private readonly ISiteService _siteService;
        private readonly IClock _clock;
        private readonly IAuditTrailManager _auditTrailManager;

        public AuditTrailTrimmingBackgroundTask(ISiteService siteService, IClock clock, IAuditTrailManager auditTrailManager) {
            _siteService = siteService;
            _clock = clock;
            _auditTrailManager = auditTrailManager;
        }

        public AuditTrailTrimmingSettingsPart Settings {
            get { return _siteService.GetSiteSettings().As<AuditTrailTrimmingSettingsPart>(); }
        }

        public void Sweep() {
            if (Monitor.TryEnter(_sweepLock)) {
                try {
                    Logger.Debug("Beginning sweep.");

                    // We don't need to check the audit trail for events to remove every minute. Let's stick with twice a day.
                    if (!TimeToTrim())
                        return;

                    Logger.Debug("Starting audit trail trimming operation.");
                    var deletedRecords = _auditTrailManager.Trim(TimeSpan.FromDays(Settings.RetentionPeriod));
                    Logger.Debug("Audit trail trimming operation completed. {0} records were deleted.", deletedRecords.Count());
                    Settings.LastRunUtc = _clock.UtcNow;
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Error during sweep.");
                }
                finally {
                    Monitor.Exit(_sweepLock);
                    Logger.Debug("Ending sweep.");
                }
            }
        }

        private bool TimeToTrim() {
            var lastRun = Settings.LastRunUtc ?? DateTime.MinValue;
            var now = _clock.UtcNow;
            var interval = TimeSpan.FromHours(12);
            return now - lastRun > interval;
        }
    }
}