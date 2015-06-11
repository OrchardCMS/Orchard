using System.Collections.Generic;
using System.Web.Mvc;
using IDeliverable.Licensing;
using IDeliverable.Licensing.Orchard;
using IDeliverable.Slides.Helpers;
using Orchard;
using Orchard.Localization;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;

namespace IDeliverable.Slides.Services
{
    public class InvalidLicenseKeyBanner : Component, INotificationProvider
    {
        private readonly ILicenseValidator _licenseValidator;
        private readonly ILicenseAccessor _licenseAccessor;
        private readonly UrlHelper _urlHelper;

        public InvalidLicenseKeyBanner(ILicenseValidator licenseValidator, ILicenseAccessor licenseAccessor, UrlHelper urlHelper)
        {
            _licenseValidator = licenseValidator;
            _licenseAccessor = licenseAccessor;
            _urlHelper = urlHelper;
        }

        public IEnumerable<NotifyEntry> GetNotifications()
        {
            var token = _licenseValidator.ValidateLicense(_licenseAccessor.GetSlidesLicense());
            if (!token.IsValid)
            {
                var url = _urlHelper.Action("Index", "License", new { area = "IDeliverable.Slides" });
                LocalizedString message;

                switch (token.Error) {
                    case LicenseValidationError.HostnameMismatch:
                        message = T("The <a href=\"{0}\">Slides license key</a> is invalid for the current host name.", url);
                        break;
                    case LicenseValidationError.LicenseExpired:
                        message = T("The <a href=\"{0}\">Slides license key</a> has expired.", url);
                        break;
                    case LicenseValidationError.LicenseRevoked:
                        message = T("The <a href=\"{0}\">Slides license key</a> has been revoked.", url);
                        break;
                    case LicenseValidationError.UnhandledException:
                        message = T("There was an error validating the <a href=\"{0}\">Slides license key</a>.", url);
                        break;
                    case LicenseValidationError.SignatureValidationFailed:
                    case LicenseValidationError.UnknownLicenseKey:
                    default:
                        message = T("The <a href=\"{0}\">Slides license key</a> is invalid.", url);
                        break;
                }
                yield return new NotifyEntry { Message = message, Type = NotifyType.Warning };
            }
        }
    }
}
