using System;
using System.Web;
using Orchard;
using Orchard.FileSystems.AppData;

namespace IDeliverable.Slides.Helpers
{
    public class LicenseValidationHelper : Licensing.Orchard.LicenseValidationHelper
    {
        public static bool GetLicenseIsValid()
        {
            try
            {
                EnsureLicenseIsValid();
                return true;
            }
            catch (Exception)
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
        protected override string LicenseKey => ""; // TODO: Get this from some kind of site settings.
        protected override bool SkipValidationForLocalRequests => true;
    }
}