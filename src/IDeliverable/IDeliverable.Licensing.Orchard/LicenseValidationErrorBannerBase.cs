using System.Collections.Generic;
using System.Linq;
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
        protected LicenseValidationErrorBannerBase(IEnumerable<ILicensedProductManifest> products, UrlHelper urlHelper, string productId)
        {
            mProducts = products;
            mUrlHelper = urlHelper;
            mProductId = productId;
        }

        private readonly IEnumerable<ILicensedProductManifest> mProducts;
        private readonly UrlHelper mUrlHelper;
        private readonly string mProductId;

        public IEnumerable<NotifyEntry> GetNotifications()
        {
            var productManifest = mProducts.Single(x => x.ProductId == mProductId);
            var licenseSettingsUrl = mUrlHelper.Action("Index", "Admin", new { area = "Settings", groupInfoId = "Licenses" });
            var refreshResultUrl = mUrlHelper.Action("VerifyLicenseKey", "LicenseValidation", new { area = productManifest.ProductName });
            var refreshResultLink = T("<a href=\"{0}\">Refresh</a>", refreshResultUrl);
            LocalizedString message = null;

            try
            {
                LicenseValidationHelper.EnsureLicenseIsValid(productManifest.ProductId);
            }
            catch (LicenseValidationException ex)
            {
                switch (ex.Error)
                {
                    case LicenseValidationError.UnknownLicenseKey:
                        message = T("The <a href=\"{0}\">configured license key</a> for the {1} module is invalid. {2}", licenseSettingsUrl, productManifest.ProductName, refreshResultLink);
                        break;
                    case LicenseValidationError.HostnameMismatch:
                        message = T("The <a href=\"{0}\">configured license key</a> for the {1} module is invalid for the current host name. {2}", licenseSettingsUrl, productManifest.ProductName, refreshResultLink);
                        break;
                    case LicenseValidationError.NoActiveSubscription:
                        message = T("The <a href=\"{0}\">configured license key</a> for the {1} module has no active subscription. {2}", licenseSettingsUrl, productManifest.ProductName, refreshResultLink);
                        break;
                    case LicenseValidationError.TokenAgeValidationFailed:
                    case LicenseValidationError.TokenSignatureValidationFailed:
                    case LicenseValidationError.LicensingServiceError:
                    case LicenseValidationError.LicensingServiceUnreachable:
                    case LicenseValidationError.UnexpectedError:
                    default:
                        message = T("There was an error validating the <a href=\"{0}\">configured license key</a> for the {1} module. {2}", licenseSettingsUrl, productManifest.ProductName, refreshResultLink);
                        break;
                }

                Logger.Warning(ex, "An error occurred while validating the configured license key for the {0} module. {1}", productManifest.ProductName, refreshResultLink);
            }

            if (message != null)
                yield return new NotifyEntry { Message = message, Type = NotifyType.Warning };
        }
    }
}
