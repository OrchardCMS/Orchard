using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using SysEnvironment = System.Environment;

namespace Orchard.OpenId.Models {
    [OrchardFeature("Orchard.OpenId.AzureActiveDirectory")]
    public class AzureActiveDirectorySettingsPart : ContentPart {
        private const char ServiceResourceIdsSeprator = '=';
        private const string ServiceResourceIdDefaultKey = "default";

        public string Tenant {
            get { return this.Retrieve(x => x.Tenant); }
            set { this.Store(x => x.Tenant, value); }
        }

        public string ADInstance {
            get { return this.Retrieve(x => x.ADInstance, () => "https://login.microsoftonline.com/{0}"); }
            set { this.Store(x => x.ADInstance, value); }
        }

        public string ClientId {
            get { return this.Retrieve(x => x.ClientId); }
            set { this.Store(x => x.ClientId, value); }
        }

        public string AppName {
            get { return this.Retrieve(x => x.AppName); }
            set { this.Store(x => x.AppName, value); }
        }

        public string LogoutRedirectUri {
            get { return this.Retrieve(x => x.LogoutRedirectUri); }
            set { this.Store(x => x.LogoutRedirectUri, value); }
        }

        public bool BearerAuthEnabled {
            get { return this.Retrieve(x => x.BearerAuthEnabled); }
            set { this.Store(x => x.BearerAuthEnabled, value); }
        }

        public bool SSLEnabled {
            get { return this.Retrieve(x => x.SSLEnabled); }
            set { this.Store(x => x.SSLEnabled, value); }
        }

        public bool AzureWebSiteProtectionEnabled {
            get { return this.Retrieve(x => x.AzureWebSiteProtectionEnabled); }
            set { this.Store(x => x.AzureWebSiteProtectionEnabled, value); }
        }

        public string GraphApiUrl {
            get { return this.Retrieve(x => x.GraphApiUrl, () => "https://graph.windows.net"); }
            set { this.Store(x => x.GraphApiUrl, value); }
        }

        public bool UseAzureGraphApi {
            get { return this.Retrieve(x => x.UseAzureGraphApi); }
            set { this.Store(x => x.UseAzureGraphApi, value); }
        }

        public string ServiceResourceID {
            get { return this.Retrieve(x => x.ServiceResourceID); }
            set { this.Store(x => x.ServiceResourceID, value); }
        }

        public string AppKey {
            get { return this.Retrieve(x => x.AppKey); }
            set { this.Store(x => x.AppKey, value); }
        }

        public string GraphApiKey {
            get { return this.Retrieve(x => x.GraphApiKey); }
            set { this.Store(x => x.GraphApiKey, value); }
        }

        public bool IsValid() {
            if (String.IsNullOrWhiteSpace(Tenant) ||
                String.IsNullOrWhiteSpace(ClientId) ||
                String.IsNullOrWhiteSpace(LogoutRedirectUri) ||
                String.IsNullOrWhiteSpace(ServiceResourceID) ||
                String.IsNullOrWhiteSpace(AppKey)) {

                return false;
            }

            return true;
        }

        public Dictionary<string, string> ServiceResourceIDs {
            get {
                return this
                    .Retrieve(x => x.ServiceResourceID)
                    .Split(SysEnvironment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                    .ToDictionary(
                        resourceId => {
                            return resourceId.Contains(ServiceResourceIdsSeprator) ?
                                        resourceId.Split(ServiceResourceIdsSeprator)[0] :
                                        ServiceResourceIdDefaultKey;
                        },
                        resourceId => {
                            return resourceId.Contains(ServiceResourceIdsSeprator) ?
                                        resourceId.Split(ServiceResourceIdsSeprator)[1] :
                                        resourceId;
                        }
                    );
            }
        }
    }
}