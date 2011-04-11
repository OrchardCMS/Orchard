using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Hosting;

using Castle.Core.Logging;

using log4net;

namespace Orchard.Logging {
    public class OrchardLog4netFactory : AbstractLoggerFactory {
        public OrchardLog4netFactory()
            : this(ConfigurationManager.AppSettings["log4net.Config"]) {
        }

        public OrchardLog4netFactory(String configFilename) {
            if (!String.IsNullOrWhiteSpace(configFilename)) {
                var mappedConfigFilename = configFilename;

                if (HostingEnvironment.IsHosted) {
                    if (!VirtualPathUtility.IsAppRelative(mappedConfigFilename)) {
                        if (!mappedConfigFilename.StartsWith("/")) {
                            mappedConfigFilename = "~/" + mappedConfigFilename;
                        }
                        else {
                            mappedConfigFilename = "~" + mappedConfigFilename;
                        }
                    }

                    mappedConfigFilename = HostingEnvironment.MapPath(mappedConfigFilename);
                }

                OrchardXmlConfigurator.Configure(mappedConfigFilename);
            }
        }

        /// <summary>
        ///   Configures log4net with a stream containing XML.
        /// </summary>
        /// <param name = "config"></param>
        public OrchardLog4netFactory(Stream config) {
            OrchardXmlConfigurator.Configure(config);
        }

        public override Castle.Core.Logging.ILogger Create(string name, LoggerLevel level) {
            throw new NotSupportedException("Logger levels cannot be set at runtime. Please review your configuration file.");
        }

        public override Castle.Core.Logging.ILogger Create(string name) {
            return new OrchardLog4netLogger(LogManager.GetLogger(name), this);
        }
    }
}
