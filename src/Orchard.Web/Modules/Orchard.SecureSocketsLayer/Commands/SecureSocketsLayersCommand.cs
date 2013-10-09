using System;
using System.Collections.Generic;
using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.SecureSocketsLayer.Models;

namespace Orchard.SecureSocketsLayer.Commands {
    public class SecureSocketsLayersCommand : DefaultOrchardCommandHandler {
        private readonly IOrchardServices _services;

        public SecureSocketsLayersCommand(IOrchardServices services) {
            _services = services;
        }

        [OrchardSwitch]
        public bool SecureEverything { get; set; }
        [OrchardSwitch]
        public bool CustomEnabled { get; set; }
        [OrchardSwitch]
        public string Urls { get; set; }
        [OrchardSwitch]
        public string SecureHostName { get; set; }
        [OrchardSwitch]
        public string InsecureHostName { get; set; }

        [CommandName("site setting set ssl")]
        [CommandHelp("site setting set ssl /SecureEverything:true /CustomEnabled:true /Urls:<value> /SecureHostName:domain.com /InsecureHostName:secure.domain.com\r\n" +
            "\tSet the 'SSL' site settings. Urls example: /Urls:\"'mysite.com/a','mysite.com/b'\"")]
        [OrchardSwitches("SecureEverything,CustomEnabled,Urls,SecureHostName,InsecureHostName")]
        public void SetSSLInfo() {
            var settings = _services.WorkContext.CurrentSite.As<SslSettingsPart>();
            if (settings == null) {
                return;
            }

            if (!string.IsNullOrWhiteSpace(Urls)) {
                var comma = false;
                var urlList = new List<string>();
                try {
                    Urls = Urls.Trim();
                    while (Urls.Length != 0) {
                        var first = Urls[0];
                        if (first == ',' && comma) {
                            Urls = Urls.Substring(1);
                            comma = false;
                        }
                        else if (first == '\'' && !comma) {
                            int end = Urls.IndexOf('\'', 1);
                            if (end == -1) {
                                throw new ArgumentException("Invalid Urls");
                            }
                            urlList.Add(Urls.Substring(1, end - 1));
                            Urls = Urls.Substring(end + 1);
                            comma = true;
                        }
                        else {
                            throw new ArgumentException("Invalid Urls");
                        }
                    }
                    if (!comma)
                        throw new ArgumentException("Invalid Urls");
                    Urls = string.Join("\r\n", urlList);
                }
                catch(ArgumentException) {
                    Context.Output.WriteLine(T("'Urls' site setting invalid"));
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(Urls)) {
                Urls = null;
            }

            settings.SecureEverything = SecureEverything;
            settings.CustomEnabled = CustomEnabled;
            settings.Urls = Urls;
            settings.SecureHostName = SecureHostName;
            settings.InsecureHostName = InsecureHostName;

            Context.Output.WriteLine(T("'Secure Everything' site setting set to '{0}'", SecureEverything));
            Context.Output.WriteLine(T("'Custom Enabled' site setting set to '{0}'", CustomEnabled));
            Context.Output.WriteLine(T("'Urls' site setting set to '{0}'", Urls));
            Context.Output.WriteLine(T("'Secure Host Name' site setting set to '{0}'", SecureHostName));
            Context.Output.WriteLine(T("'Insecure Host Name' site setting set to '{0}'", InsecureHostName));
        }
    }
}