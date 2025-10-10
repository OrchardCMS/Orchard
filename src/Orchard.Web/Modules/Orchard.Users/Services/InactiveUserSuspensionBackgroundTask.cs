using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Services;
using Orchard.Settings;
using Orchard.Tasks;
using Orchard.Tasks.Locking.Services;
using Orchard.Users.Events;
using Orchard.Users.Models;

namespace Orchard.Users.Services {
    public class InactiveUserSuspensionBackgroundTask : Component, IBackgroundTask {

        private readonly IDistributedLockService _distributedLockService;
        private readonly ISiteService _siteService;
        private readonly IClock _clock;
        private readonly IContentManager _contentManager;
        private readonly IUserEventHandler _userEventHandlers;
        private readonly IAuthorizationService _authorizationService;
        private readonly IEnumerable<IUserSuspensionConditionProvider> _userSuspensionConditionProviders;

        public InactiveUserSuspensionBackgroundTask(
            IDistributedLockService distributedLockService,
            ISiteService siteService,
            IClock clock,
            IContentManager contentManager,
            IUserEventHandler userEventHandlers,
            IAuthorizationService authorizationService,
            IEnumerable<IUserSuspensionConditionProvider> userSuspensionConditionProviders) {

            _distributedLockService = distributedLockService;
            _siteService = siteService;
            _clock = clock;
            _contentManager = contentManager;
            _userEventHandlers = userEventHandlers;
            _authorizationService = authorizationService;
            _userSuspensionConditionProviders = userSuspensionConditionProviders;
        }


        public void Sweep() {
            Logger.Debug("Beginning sweep to suspend inactive users.");
            try {
                // Only allow this task to run on one farm node at a time.
                IDistributedLock @lock;
                if (_distributedLockService.TryAcquireLock(GetType().FullName, TimeSpan.FromHours(1), out @lock)) {
                    using (@lock) {
                        // Check whether it's time to do another sweep.
                        if (!IsItTimeToSweep()) {
                            return; // too soon
                        }

                        Logger.Debug("Checking for inactive users.");
                        // Get all inactive users (last logon older than the configured timespan)
                        var thresholdDate = _clock.UtcNow - TimeSpan.FromDays(GetSettings().AllowedInactivityDays);
                        IContentQuery<UserPart> inactiveUsersQuery = _contentManager
                            .Query<UserPart>()
                            .Where<UserPartRecord>(upr =>
                                // user is enabled
                                upr.RegistrationStatus == UserStatus.Approved
                                && upr.EmailStatus == UserStatus.Approved)
                            .Where<UserPartRecord>(upr =>
                                // The last login happened a long time ago
                                (upr.LastLoginUtc != null && upr.LastLoginUtc <= thresholdDate)
                                // The user never logged in AND was created a long time ago
                                || (upr.LastLoginUtc == null && upr.CreatedUtc <= thresholdDate)
                                );
                        // If providers could alter the query we'd be able to immediately limit the number
                        // of ContentItems we'll fetch, and as a consequence the number of operations later.
                        // However, such conditions would make users immune from being suspended.
                        foreach (var provider in _userSuspensionConditionProviders) {
                            inactiveUsersQuery = provider.AlterQuery(inactiveUsersQuery);
                        }
                        var inactiveUsers = inactiveUsersQuery.List();
                        // By default, all inactive users should be suspended, except SiteOwner.
                        foreach (var userUnderTest in inactiveUsers.Where(up => !IsSiteOwner(up))) {

                            // Ask providers whether users should be processed/disabled
                            var saveTheUser = _userSuspensionConditionProviders
                                .Aggregate(false, (prev, scp) => prev || scp.UserIsProtected(userUnderTest));

                            // Suspend the users that have gotten this far.
                            if (!saveTheUser) {
                                DisableUser(userUnderTest);
                            }
                        }
                        // Done!
                        // Mark the time that we have done this check.
                        GetSettings().LastSweepUtc = _clock.UtcNow;
                        Logger.Debug("Done checking for inactive users.");
                    }
                } else {
                    Logger.Debug("Distributed lock could not be acquired; going back to sleep.");
                }

            } catch (Exception ex) {
                Logger.Error(ex, "Error during sweep to suspend inactive users.");
            } finally {
                Logger.Debug("Ending sweep to suspend inactive users.");
            }
        }

        private bool IsSiteOwner(UserPart userPart) {
            return _authorizationService
                .TryCheckAccess(StandardPermissions.SiteOwner,
                    userPart, null);
        }

        private void DisableUser(UserPart userPart) {
            userPart.RegistrationStatus = UserStatus.Pending;
            Logger.Information(T("User {0} disabled by automatic moderation", userPart.UserName).Text);
            _userEventHandlers.Moderate(userPart);
        }

        private bool IsItTimeToSweep() {
            var settings = GetSettings();
            if (settings.SuspendInactiveUsers) {
                if (settings.MinimumSweepInterval <= 0) {
                    return true;
                }
                var lastSweep = settings.LastSweepUtc ?? DateTime.MinValue;
                var now = _clock.UtcNow;
                var interval = TimeSpan.FromHours(settings.MinimumSweepInterval);
                return now - lastSweep >= interval;
            }
            return false;
        }

        #region Memorize settings
        private ISite _siteSettings;
        private ISite GetSiteSettings() {
            if (_siteSettings == null) {
                _siteSettings = _siteService.GetSiteSettings();
            }
            return _siteSettings;
        }
        private UserSuspensionSettingsPart _settingsPart;
        private UserSuspensionSettingsPart GetSettings() {
            if (_settingsPart == null) {
                _settingsPart = GetSiteSettings().As<UserSuspensionSettingsPart>();
            }
            return _settingsPart;
        }
        #endregion
    }
}