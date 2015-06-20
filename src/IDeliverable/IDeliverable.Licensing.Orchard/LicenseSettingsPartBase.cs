using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace IDeliverable.Licensing.Orchard
{
    public abstract class LicenseSettingsPartBase : ContentPart
    {
        public readonly LazyField<string> mProductVersionField = new LazyField<string>();
        public abstract string ProductId { get; }

        public string LicenseKey
        {
            get { return this.Retrieve(x => x.LicenseKey); }
            set { this.Store(x => x.LicenseKey, value); }
        }

        public string ProductVersion => mProductVersionField.Value;
    }
}