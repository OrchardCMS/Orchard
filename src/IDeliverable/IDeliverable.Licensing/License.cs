namespace IDeliverable.Licensing {
    public class License : ILicense {
        public int ProductId { get; set; }
        public string Key { get; set; }
    }
}