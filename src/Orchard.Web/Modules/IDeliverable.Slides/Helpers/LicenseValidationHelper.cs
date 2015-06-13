using System;
using System.Web;
using IDeliverable.Licensing.Orchard;
using Orchard;
using Orchard.FileSystems.AppData;

namespace IDeliverable.Slides.Helpers
{
    public class LicenseValidationHelper : LicenseValidationHelperBase
    {
        public static bool GetLicenseIsValid()
        {
            try
            {
                EnsureLicenseIsValid();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void EnsureLicenseIsValid()
        {
            Instance.ValidateLicense();
        }

        private static LicenseValidationHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    var appDataFolder = HttpContext.Current.Request.RequestContext.GetWorkContext().Resolve<IAppDataFolder>();
                    _instance = new LicenseValidationHelper(appDataFolder);
                }

                return _instance;
            }
        }

        private static LicenseValidationHelper _instance;

        public LicenseValidationHelper(IAppDataFolder appDataFolder)
            :base(appDataFolder)
        {
        }

        protected override string ProductId => "233554";
        protected override string LicenseKey => ""; // TODO: Sipke get this from some kind of site settings.
        protected override bool SkipValidationForLocalRequests => true;
    }
}