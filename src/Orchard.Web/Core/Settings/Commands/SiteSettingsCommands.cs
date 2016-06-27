using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.Core.Settings.Models;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Settings;
using Orchard.Utility.Extensions;

namespace Orchard.Core.Settings.Commands {
    public class SiteSettingsCommands : DefaultOrchardCommandHandler {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISiteService _siteService;

        public SiteSettingsCommands(IHttpContextAccessor httpContextAccessor, ISiteService siteService) {
            _httpContextAccessor = httpContextAccessor;
            _siteService = siteService;
        }

        [OrchardSwitch]
        public string BaseUrl { get; set; }

        [OrchardSwitch]
        public bool Force { get; set; }

        [CommandName("site setting set baseurl")]
        [CommandHelp("site setting set baseurl [/BaseUrl:baseUrl] [/Force:true]\r\n\tSet the 'BaseUrl' site settings. If no base url is provided, " +
            "use the current request context heuristic to discover the base url. " +
            "If 'Force' is true, set the site base url even if it is already set. " +
            "The default behavior is to not override the setting.")]
        [OrchardSwitches("BaseUrl,Force")]
        public void SetBaseUrl() {
            // Don't do anything if set and not forcing
            if (!string.IsNullOrEmpty(_siteService.GetSiteSettings().BaseUrl)) {
                if (!Force) {
                    Context.Output.WriteLine(T("'BaseUrl' site setting is already set. Use the 'Force' flag to override."));
                    return;
                }
            }

            // Retrieve request URL if BaseUrl not provided as a switch value
            if (string.IsNullOrEmpty(BaseUrl)) {
                if (_httpContextAccessor.Current().IsBackgroundContext()) {
                    Context.Output.WriteLine(T("No HTTP request available to determine the base url of the site"));
                    return;
                }
                var request = _httpContextAccessor.Current().Request;
                BaseUrl = request.ToApplicationRootUrlString();
            }

            // Update base url
            _siteService.GetSiteSettings().As<SiteSettingsPart>().BaseUrl = BaseUrl;
            Context.Output.WriteLine(T("'BaseUrl' site setting set to '{0}'", BaseUrl));
        }
    }
}