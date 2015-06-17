using IDeliverable.Licensing.Orchard;
using Orchard.UI.Notify;

namespace IDeliverable.Slides.Licensing
{
    public class LicenseValidationController : LicenseValidationControllerBase
    {
        public LicenseValidationController(INotifier notifier) 
            : base(notifier)
        {
        }
        
        protected override string ProductId => LicensedProductManifest.ProductId;
    }
}