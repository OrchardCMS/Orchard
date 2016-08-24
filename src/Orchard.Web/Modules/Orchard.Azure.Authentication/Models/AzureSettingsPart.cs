using Orchard.ContentManagement;

namespace Orchard.Azure.Authentication.Models {
    public class AzureSettingsPart : ContentPart{
        public string Tenant {
            get { return this.Retrieve(x => x.Tenant); }
            set { this.Store(x => x.Tenant, value); }
        }

        public string ADInstance {
            get { return this.Retrieve(x => x.ADInstance); }
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
            get { return this.Retrieve(x => x.GraphApiUrl); }
            set { this.Store(x => x.GraphApiUrl, value); }
        }

        public bool UseAzureGraphApi {
            get { return this.Retrieve(x => x.UseAzureGraphApi); }
            set { this.Store(x => x.UseAzureGraphApi, value); }
        }
    }
}