using System.Web;
using IDeliverable.Licensing.Orchard;
using IDeliverable.Slides.Models;
using Orchard;
using Orchard.ContentManagement;
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
                    var workContext = HttpContext.Current.Request.RequestContext.GetWorkContext();
                    var appDataFolder = workContext.Resolve<IAppDataFolder>();
                    var orchardServices = workContext.Resolve<IOrchardServices>();
                    _instance = new LicenseValidationHelper(appDataFolder, orchardServices);
                }

                return _instance;
            }
        }

        private static LicenseValidationHelper _instance;

        public LicenseValidationHelper(IAppDataFolder appDataFolder, IOrchardServices orchardServices)
            : base(appDataFolder, orchardServices)
        {
        }

        protected override string ProductId => "233554";
        protected override string LicenseKey => CurrentSite.As<SlidesLicenseSettingsPart>().LicenseKey;
        protected override bool SkipValidationForLocalRequests => true;
    }
}