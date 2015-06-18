using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using IDeliverable.Licensing.Exceptions;
using IDeliverable.Licensing.VerificationTokens;
using Newtonsoft.Json.Linq;

namespace IDeliverable.Licensing.Service.Services
{
    internal class LicenseService
    {
        public LicenseService(string sendOwlApiEndpoint, string sendOwlApiKey, string sendOwlApiSecret, string tokenSigningCertificateThumbprint)
        {
            mSendOwlApiEndpoint = sendOwlApiEndpoint;
            mSendOwlApiKey = sendOwlApiKey;
            mSendOwlApiSecret = sendOwlApiSecret;
            mTokenSigningCertificateThumbprint = tokenSigningCertificateThumbprint;
            mLogger = new Logger(nameof(LicenseService));
        }

        private readonly string mSendOwlApiEndpoint;
        private readonly string mSendOwlApiKey;
        private readonly string mSendOwlApiSecret;
        private readonly string mTokenSigningCertificateThumbprint;
        private readonly Logger mLogger;

        public LicenseVerificationToken VerifyLicense(string licenseKey, string productId, string hostname, bool throwOnError)
        {
            mLogger.Info($"Starting license verification... LicenseKey={licenseKey}; ProductId={productId}; Hostname={hostname};");

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(mSendOwlApiEndpoint);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", CreateBasicAuthenticationToken(mSendOwlApiKey, mSendOwlApiSecret));
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var licenses = ParseLicenseInfo(client.GetAsync($"products/{productId}/licenses/check_valid?key={licenseKey}").Result.Content.ReadAsStringAsync().Result).ToList();

                    if (!licenses.Any())
                        throw new LicenseVerificationException($"No license with key '{licenseKey}' was found.", LicenseVerificationError.UnknownLicenseKey);

                    var orderId = licenses.First(x => x.Key == licenseKey).OrderId;
                    var order = ParseOrderInfo(client.GetAsync($"orders/{orderId}").Result.Content.ReadAsStringAsync().Result);

                    if (!order.IsProductAccessAllowed)
                    {
                        if(order.SubscriptionId != null)
                            throw new LicenseVerificationException($"The license with key '{licenseKey}' is not associated with an active subscription.", LicenseVerificationError.NoActiveSubscription);

                        throw new LicenseVerificationException($"The license with key '{licenseKey}' has been revoked.", LicenseVerificationError.LicenseRevoked);
                    }

                    if (!order.Hostnames.Contains(hostname, StringComparer.OrdinalIgnoreCase))
                        throw new LicenseVerificationException($"The license with key '{licenseKey}' is not valid for the provided '{hostname}'. Valid hostnames are '{String.Join(",", order.Hostnames)}", LicenseVerificationError.HostnameMismatch);
                }
            }
            catch (Exception ex) when (!(ex is LicenseVerificationException))
            {
                mLogger.Error($"An error occurred while verifying license with SendOwl. LicenseKey={licenseKey}; ProductId={productId}; Hostname={hostname};", ex);

                if (throwOnError)
                    throw;

                mLogger.Info("Ignoring error and issuing verification token anyway. LicenseKey={licenseKey}; ProductId={productId}; Hostname={hostname};");
            }

            mLogger.Info($"License verification finished. LicenseKey={licenseKey}; ProductId={productId}; Hostname={hostname};");

            return CreateVerificationToken(productId, hostname, licenseKey);
        }

        private LicenseVerificationToken CreateVerificationToken(string productId, string hostname, string key)
        {
            X509Certificate2 signingCert = null;

            // Try to find the certificate in either the CurrentUser or LocalMachine stores
            // to maximize code portability.
            signingCert = GetSigningCertificateFrom(StoreLocation.CurrentUser) ?? GetSigningCertificateFrom(StoreLocation.LocalMachine);
            if (signingCert == null)
                throw new Exception($"No certificate with thumbprint '{mTokenSigningCertificateThumbprint}' was found in the certificate store.");

            var info = new LicenseVerificationInfo(productId, hostname, key, DateTime.UtcNow.Ticks);
            var token = LicenseVerificationToken.Create(info, signingCert);

            return token;
        }

        private X509Certificate2 GetSigningCertificateFrom(StoreLocation location)
        {
            var store = new X509Store(StoreName.My, location);
            store.Open(OpenFlags.ReadOnly);

            try
            {
                var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, mTokenSigningCertificateThumbprint, validOnly: false);
                return certificates.Count > 0 ? certificates[0] : null;
            }
            finally
            {
                store.Close();
            }
        }

        private static string CreateBasicAuthenticationToken(string userName, string password)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}"), Base64FormattingOptions.None);
        }

        private static IEnumerable<LicenseInfo> ParseLicenseInfo(string json)
        {
            var licensesQuery =
                from node in JArray.Parse(json)
                select new LicenseInfo((string)node["license"]["key"], (int)node["license"]["order_id"]);

            return licensesQuery.ToArray();
        }

        private static OrderInfo ParseOrderInfo(string json)
        {
            var order = JObject.Parse(json)["order"];
            var status = ReadOrderStatus(order);
            var isAccessAllowed = (bool) order["access_allowed"];
            var subscriptionId = (int?) order["subscription_id"];
            var customFields = (JArray)order["order_custom_checkout_fields"];
            var hostnames = new List<string>();

            if (customFields.Count > 0)
                hostnames.Add((string)customFields[0]["order_custom_checkout_field"]["value"]);

            if (customFields.Count > 1)
                hostnames.Add((string)customFields[1]["order_custom_checkout_field"]["value"]);

            return new OrderInfo((int)order["id"], status, isAccessAllowed, subscriptionId, hostnames);
        }

        private static OrderStatus ReadOrderStatus(JToken order)
        {
            var dictionary = new Dictionary<string, OrderStatus>
            {
                {"initial", OrderStatus.Initial },
                {"payment_started", OrderStatus.PaymentStarted },
                {"payment_pending", OrderStatus.PaymentPending },
                {"failed", OrderStatus.Failed },
                {"complete", OrderStatus.Complete },
                {"chargeback", OrderStatus.Chargeback },
                {"refunded", OrderStatus.Refunded },
                {"in_dispute", OrderStatus.InDispute },
                {"free", OrderStatus.Free },
                {"imported", OrderStatus.Imported },
                {"fraud_review", OrderStatus.FraudReview },
                {"subscription_setup", OrderStatus.SubscriptionSetup },
                {"subscription_active", OrderStatus.SubscriptionActive },
                {"subscription_complete", OrderStatus.SubscriptionComplete },
                {"subscription_cancelled", OrderStatus.SubscriptionCancelled }
            };
            var status = (string) order["state"];

            return dictionary.ContainsKey(status) ? dictionary[status] : OrderStatus.Unknown;
        }
    }
}