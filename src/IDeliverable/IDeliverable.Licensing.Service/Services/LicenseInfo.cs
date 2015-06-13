namespace IDeliverable.Licensing.Service.Services
{
    public class LicenseInfo
    {
        public LicenseInfo(string key, int orderId)
        {
            Key = key;
            OrderId = orderId;
        }

        public string Key { get; private set; }
        public int OrderId { get; private set; }
    }
}