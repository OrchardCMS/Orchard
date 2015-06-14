using Orchard.ContentManagement;

namespace IDeliverable.Licensing.Orchard
{
    public abstract class LicenseSettingsPartBase : ContentPart
    {
        public string LicenseKey
        {
            get { return this.Retrieve(x => x.LicenseKey); }
            set { this.Store(x => x.LicenseKey, value); }
        }
    }
}