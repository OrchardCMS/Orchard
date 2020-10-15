using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Orchard.Azure.MediaServices.Helpers
{
    public class StorageHelper
    {
        public static async Task<IEnumerable<string>> EnsureCorsIsEnabledAsync(string amsAccountName, string amsAccountKey, string storageAccountKey, params string[] origins)
        {
            if (!string.IsNullOrWhiteSpace(amsAccountKey) &&
                !string.IsNullOrWhiteSpace(amsAccountName) &&
                !string.IsNullOrWhiteSpace(storageAccountKey))
            {
                var context = new CloudMediaContext(amsAccountName, amsAccountKey);
                var storageAccount = new CloudStorageAccount(new StorageCredentials(context.DefaultStorageAccount.Name, storageAccountKey), false);
                var client = storageAccount.CreateCloudBlobClient();
                var serviceProperties = await client.GetServicePropertiesAsync().ConfigureAwait(continueOnCapturedContext: false);
                var requiredHeaders = new[] { "accept", "x-ms-blob-content-type", "x-ms-blob-type", "x-ms-date", "x-ms-version", "content-disposition", "content-length", "content-range", "content-type" };
                var requiredMethods = CorsHttpMethods.Put | CorsHttpMethods.Options;

                if (serviceProperties.Cors == null)
                    serviceProperties.Cors = new CorsProperties();

                var rule = FindBestMatchingRule(serviceProperties, requiredHeaders, requiredMethods, origins);

                if (rule == null)
                {
                    rule = new CorsRule
                    {
                        AllowedHeaders = requiredHeaders,
                        AllowedMethods = requiredMethods,
                        AllowedOrigins = new List<string>(),
                        ExposedHeaders = new List<string> { "*" },
                        MaxAgeInSeconds = 1800 // 30 minutes
                    };

                    serviceProperties.Cors.CorsRules.Add(rule);
                }

                var addedOrigins = new List<string>();
                var settingsChanged = false;

                foreach (var origin in origins.Where(origin => !rule.AllowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase)))
                {
                    rule.AllowedOrigins.Add(origin);
                    addedOrigins.Add(origin);
                    settingsChanged = true;
                }

                if (settingsChanged)
                    await client.SetServicePropertiesAsync(serviceProperties).ConfigureAwait(continueOnCapturedContext: false);

                return addedOrigins.AsEnumerable();
            }

            return new List<string>().AsEnumerable();
        }

        private static CorsRule FindBestMatchingRule(ServiceProperties serviceProperties, IEnumerable<string> requiredHeaders, CorsHttpMethods requiredMethods, IEnumerable<string> origins)
        {
            var query =
                from rule in serviceProperties.Cors.CorsRules
                let hasRequiredHeadersAndMethods = (rule.AllowedHeaders.Contains("*") || requiredHeaders.All(rule.AllowedHeaders.Contains)) && (rule.AllowedMethods & requiredMethods) == requiredMethods
                let numberMatchingOrigins = origins.Count(x => rule.AllowedOrigins.Contains(x))
                where hasRequiredHeadersAndMethods
                orderby hasRequiredHeadersAndMethods descending, numberMatchingOrigins descending
                select rule;

            return query.FirstOrDefault();
        }
    }
}