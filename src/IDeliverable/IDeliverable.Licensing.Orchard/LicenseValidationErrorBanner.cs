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
    public class LicenseValidationErrorBanner : Component, INotificationProvider
    {
        protected LicenseValidationErrorBanner(IEnumerable<ILicensedProductManifest> products, UrlHelper urlHelper)
        {
            _products = products;
            _urlHelper = urlHelper;
        }

        private readonly IEnumerable<ILicensedProductManifest> _products;
        private readonly UrlHelper _urlHelper;

        public IEnumerable<NotifyEntry> GetNotifications()
        {
            var licenseSettingsUrl = _urlHelper.Action("Index", "Admin", new { area = "Settings", groupInfoId = "Licenses" });
            LocalizedString message = null;

            foreach (var productManifest in _products)
            {
                try
                {
                    LicenseValidationHelper.EnsureLicenseIsValid(productManifest.ProductId);
                }
                catch (LicenseValidationException ex)
                {
                    switch (ex.Error)
                    {
                        case LicenseValidationError.UnknownLicenseKey:
                            message = T("The <a href=\"{0}\">configured license key</a> for the {1} module is invalid.", licenseSettingsUrl, productManifest.ProductName);
                            break;
                        case LicenseValidationError.HostnameMismatch:
                            message = T("The <a href=\"{0}\">configured license key</a> for the {1} module is invalid for the current host name.", licenseSettingsUrl, productManifest.ProductName);
                            break;
                        case LicenseValidationError.TokenSignatureValidationFailed:
                        case LicenseValidationError.LicensingServiceError:
                        case LicenseValidationError.LicensingServiceUnreachable:
                        case LicenseValidationError.UnexpectedError:
                        default:
                            message = T("There was an error validating the <a href=\"{0}\">configured license key</a> for the {1} module.", licenseSettingsUrl, productManifest.ProductName);
                            break;
                    }

                    Logger.Warning(ex, "An error occurred while validating the configured license key for the {0} module.", productManifest.ProductName);
                }

                if (message != null)
                    yield return new NotifyEntry { Message = message, Type = NotifyType.Warning };
            }
        }
    }
}
