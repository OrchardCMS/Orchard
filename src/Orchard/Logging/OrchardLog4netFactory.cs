using System;
using System.Configuration;
using Castle.Core.Logging;
using log4net;
using log4net.Config;
using Orchard.Environment;

namespace Orchard.Logging {
    public class OrchardLog4netFactory : AbstractLoggerFactory {
        private static bool _isFileWatched = false;

        public OrchardLog4netFactory(IHostEnvironment hostEnvironment) 
            : this(ConfigurationManager.AppSettings["log4net.Config"], hostEnvironment) { }

        public OrchardLog4netFactory(string configFilename, IHostEnvironment hostEnvironment) {
            if (!_isFileWatched && !string.IsNullOrWhiteSpace(configFilename)) {
                // Only monitor configuration file in full trust
                XmlConfigurator.ConfigureAndWatch(GetConfigFile(configFilename));
                _isFileWatched = true;
            }
        }

        public override Castle.Core.Logging.ILogger Create(string name, LoggerLevel level) {
            throw new NotSupportedException("Logger levels cannot be set at runtime. Please review your configuration file.");
        }

        public override Castle.Core.Logging.ILogger Create(string name) {
            return new OrchardLog4netLogger(LogManager.GetLogger(name), this);
        }
    }
}
