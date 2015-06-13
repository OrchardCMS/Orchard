using System.Collections.Generic;
using System.Web.Mvc;
using IDeliverable.Licensing.Validation;
using Orchard;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;

namespace IDeliverable.Licensing.Orchard
{
    public abstract class LicenseValidationErrorBannerBase : Component, INotificationProvider
    {
        public LicenseValidationErrorBannerBase(string moduleName, UrlHelper urlHelper)
        {
            _moduleName = moduleName;
            _urlHelper = urlHelper;
        }

        private readonly string _moduleName;
        private readonly UrlHelper _urlHelper;

        protected abstract void EnsureLicenseIsValid();

        public IEnumerable<NotifyEntry> GetNotifications()
        {
            var licenseSettingsUrl = _urlHelper.Action("Index", "Home", new { area = "Settings" }); // TODO: Sipke change this to construct the right URL given the settings group "Licenses".
            LocalizedString message = null;

            try
            {
                EnsureLicenseIsValid();
            }
            catch (LicenseValidationException ex)
            {
                switch (ex.Error)
                {
                    case LicenseValidationError.UnknownLicenseKey:
                        message = T("The <a href=\"{0}\">configured license key</a> for the {1} module is invalid.", licenseSettingsUrl, _moduleName);
                        break;
                    case LicenseValidationError.HostnameMismatch:
                        message = T("The <a href=\"{0}\">configured license key</a> for the {1} module is invalid for the current host name.", licenseSettingsUrl, _moduleName);
                        break;
                    case LicenseValidationError.TokenSignatureValidationFailed:
                    case LicenseValidationError.LicensingServiceError:
                    case LicenseValidationError.LicensingServiceUnreachable:
                    case LicenseValidationError.UnexpectedError:
                    default:
                        message = T("There was an error validating the <a href=\"{0}\">configured license key</a> for the {1} module.", licenseSettingsUrl, _moduleName);
                        break;
                }

                Logger.Warning(ex, "An error occurred while validating the configured license key for the {0} module.", _moduleName);
            }

            if (message != null)
                yield return new NotifyEntry { Message = message, Type = NotifyType.Warning };
        }
    }
}
