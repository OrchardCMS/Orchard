using System;
using System.IO;
using log4net;
using log4net.Config;

namespace IDeliverable.Licensing.Service.Services
{
    public class Logger
    {
        private const string ConfigFileName = "Log4net.config";

        static Logger()
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var configFilePath = Path.Combine(basePath, ConfigFileName);
            if (File.Exists(configFilePath))
                XmlConfigurator.Configure(new FileInfo(configFilePath));
            else
                throw new Exception($"The log4net configuration file could not be found at path '{configFilePath}'.");
        }

        public Logger(Type loggerType)
            :this(loggerType.FullName)
        {
        }

        public Logger(string loggerName)
        {
            mLog = LogManager.GetLogger(loggerName);
        }

        private ILog mLog;

        public void Fatal(string message, Exception ex = null)
        {
            if (mLog.IsFatalEnabled)
                mLog.Fatal(message, ex);
        }

        public void Error(string message, Exception ex = null)
        {
            if (mLog.IsErrorEnabled)
                mLog.Error(message, ex);
        }

        public void Warn(string message, Exception ex = null)
        {
            if (mLog.IsWarnEnabled)
                mLog.Warn(message, ex);
        }

        public void Info(string message, Exception ex = null)
        {
            if (mLog.IsInfoEnabled)
                mLog.Info(message, ex);
        }

        public void Debug(string message, Exception ex = null)
        {
            if (mLog.IsDebugEnabled)
                mLog.Debug(message, ex);
        }
    }
}