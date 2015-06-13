using System.Collections.Generic;
using System.Web.Mvc;
using IDeliverable.Licensing.Validation;
using IDeliverable.Slides.Helpers;
using Orchard;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;

namespace IDeliverable.Slides.Services
{
    public class InvalidLicenseKeyBanner : Component, INotificationProvider
    {
        public InvalidLicenseKeyBanner(UrlHelper urlHelper)
        {
            _urlHelper = urlHelper;

        }

        private readonly UrlHelper _urlHelper;

        public IEnumerable<NotifyEntry> GetNotifications()
        {
            var licenseSettingsUrl = _urlHelper.Action("Index", "License", new { area = "IDeliverable.Slides" });
            LocalizedString message = null;

            try
            {
                LicenseValidationHelper.EnsureLicenseIsValid();
            }
            catch (LicenseValidationException ex)
            {
                switch (ex.Error)
                {
                    case LicenseValidationError.UnknownLicenseKey:
                        message = T("The <a href=\"{0}\">configured license key</a> is invalid.", licenseSettingsUrl);
                        break;
                    case LicenseValidationError.HostnameMismatch:
                        message = T("The <a href=\"{0}\">configured license key</a> is invalid for the current host name.", licenseSettingsUrl);
                        break;
                    case LicenseValidationError.TokenSignatureValidationFailed:
                    case LicenseValidationError.LicensingServiceError:
                    case LicenseValidationError.LicensingServiceUnreachable:
                    case LicenseValidationError.UnexpectedError:
                    default:
                        message = T("There was an error validating the <a href=\"{0}\">configured license key</a>.", licenseSettingsUrl);
                        break;
                }

                Logger.Warning(ex, "An error occurred while validating the configured license key.");
            }

            if (message != null)
                yield return new NotifyEntry { Message = message, Type = NotifyType.Warning };
        }
    }
}
