using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
    // [OrchardFeature("AutomatedUserModeration")]?
    public class InactiveUserSuspensionBackgroundTask : Component, IBackgroundTask {

        private readonly IDistributedLockService _distributedLockService;
        private readonly ISiteService _siteService;
        private readonly IClock _clock;
        private readonly IContentManager _contentManager;
        private readonly IUserEventHandler _userEventHandlers;
        private readonly IAuthorizationService _authorizationService;

        public InactiveUserSuspensionBackgroundTask(
            IDistributedLockService distributedLockService,
            ISiteService siteService,
            IClock clock,
            IContentManager contentManager,
            IUserEventHandler userEventHandlers,
            IAuthorizationService authorizationService) {

            _distributedLockService = distributedLockService;
            _siteService = siteService;
            _clock = clock;
            _contentManager = contentManager;
            _userEventHandlers = userEventHandlers;
            _authorizationService = authorizationService;
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
                        var thresholdDate = _clock.UtcNow - TimeSpan.FromDays(GetSettings().MinimumSweepInterval);
                        var inactiveUsersQuery = _contentManager
                            .Query<UserPart, UserPartRecord>()
                            .Where(upr => upr.RegistrationStatus == UserStatus.Approved
                                && upr.EmailStatus == UserStatus.Approved
                                && upr.LastLoginUtc >= thresholdDate);
                        // If providers could alter the query we'd be able to immediately limit the number
                        // of ContentItems we'll fetch, and as a consequence the number of operations later.
                        // However, such conditions would make users immune from being suspended.
                        var inactiveUsers = inactiveUsersQuery.List();
                        // By default, all inactive users should be suspended, except SiteOwner.
                        foreach (var userUnderTest in inactiveUsers.Where(up => !IsSiteOwner(up))) {

                            // Ask providers whether users should be processed
                            // - Site owners should be left alone
                            // - By Role:
                            //   - e.g. Authenticated should not be disabled, but Editor should
                            // - Flag on the User

                            // One way to set this up:
                            // For each user we consider two "scores" initialized at int.MinValue:
                            // - SuspendScore
                            // - SafeScore
                            // At the end if SuspendSore >= SafeScore we suspend the user.
                            // - We have already excluded users with the SiteOwner permission from this.
                            // - Each provider should work on principle with a specific value it can use,
                            //   and it would always assign that value to either SafeScore or SuspendScore
                            //   for the user under test. For security, suspending the user should usually
                            //   take priority, hence the condition for suspension is SuspendScore >= SafeScore.
                            // The value/score assigned by a specific provider is a measure of how specific
                            // its corresponding configuration/test is for a user. If for a user we have
                            // already assigned a score that is higher than what a provider would use, then
                            // that provider won't run and especially won't reassign the score.
                            // For example:
                            // - RoleProvider is based on a configuration for user Roles.
                            //   - It uses a score of 100.
                            //   - In its configuration, each role can be marked as either Protected,
                            //     ToSuspend, Neither.
                            //   - If a user being process has a Protected role, RoleProvider wants to assign
                            //     100 to their SafeScore.
                            //   - If a user under test has a ToSuspend role, RoleProvider wants to assign
                            //     100 to their SuspendScore.
                            //   - Roles that aren't marked as Protected or ToSuspend are ignored in this
                            //     RoleProvider.
                            //   - According to this RoleProvider, if a user under test has both Protected
                            //     and ToSuspend roles, then they should be suspendend, because that would
                            //     result in SuspendScore == SafeScore.
                            // - FlagProvider is based on a "NeverSuspendThisUser" flag on a ContentPart
                            //   attached to Users.
                            //   - It has a score of 200. This is higher than the RoleProvider above, because
                            //     the flag on the single user is more specific.
                            //   - If a user under test has that flag set, FlagProvider wants to assign 200
                            //     to their SafeScore.
                            var suspendScore = int.MinValue;
                            var safeScore = int.MinValue;

                            // Use providers to determine the "suspension" operation?
                            // - Disable user?
                            // - Remove role?

                            // Suspend the users that have gotten this far.
                            if (suspendScore >= safeScore) {
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
            if (settings.SuspendInactiveUsers && settings.MinimumSweepInterval > 0) {
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