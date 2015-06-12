namespace IDeliverable.Licensing.Orchard.Services {
    public interface ILicensingServiceClient {
        /// <summary>
        /// Returns a license validation token from the licensing service.
        /// </summary>
        LicenseValidationToken GetToken(ILicense license);

        /// <summary>
        /// Returns a license validation token from the licensing service.
        /// </summary>
        LicenseValidationToken GetToken(int productId, string key);
    }
}