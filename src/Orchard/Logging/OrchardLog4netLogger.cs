using System;
using System.Globalization;
using System.Web;
using log4net;
using log4net.Core;
using log4net.Util;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Logger = Castle.Core.Logging.ILogger;

namespace Orchard.Logging
{
    [Serializable]
    public class OrchardLog4netLogger : MarshalByRefObject, Logger, IShim
    {
        private static readonly Type declaringType = typeof(OrchardLog4netLogger);

        private readonly Lazy<ShellSettings> _shellSettings;

        public IOrchardHostContainer HostContainer { get; set; }

        public OrchardLog4netLogger(log4net.Core.ILogger logger, OrchardLog4netFactory factory)
        {
            OrchardHostContainerRegistry.RegisterShim(this);
            Logger = logger;
            Factory = factory;

            _shellSettings = new Lazy<ShellSettings>(LoadSettings, System.Threading.LazyThreadSafetyMode.PublicationOnly);
        }

        internal OrchardLog4netLogger()
        {
        }

        internal OrchardLog4netLogger(ILog log, OrchardLog4netFactory factory)
            : this(log.Logger, factory)
        {
        }

        private ShellSettings LoadSettings()
        {
            var ctx = HttpContext.Current;
            if (ctx == null)
                return null;

            var runningShellTable = HostContainer.Resolve<IRunningShellTable>();
            if (runningShellTable == null)
                return null;

            var shellSettings = runningShellTable.Match(new HttpContextWrapper(ctx));
            if (shellSettings == null)
                return null;

            var orchardHost = HostContainer.Resolve<IOrchardHost>();
            if (orchardHost == null)
                return null;

            var shellContext = orchardHost.GetShellContext(shellSettings);
            if (shellContext == null || shellContext.Settings == null)
                return null;


            return shellContext.Settings;
        }

        // Load the log4net thread with additional properties if they are available
        protected internal void AddExtendedThreadInfo()
        {
            if (_shellSettings.Value != null)
            {
                ThreadContext.Properties["Tenant"] = _shellSettings.Value.Name;
            }
            else
            {
                ThreadContext.Properties.Remove("Tenant");
            }

            try
            {
                var ctx = HttpContext.Current;
                if (ctx != null)
                {
                    ThreadContext.Properties["Url"] = ctx.Request.Url.ToString();
                }
                else
                {
                    ThreadContext.Properties.Remove("Url");
                }
            }
            catch (HttpException)
            {
                // can happen on cloud service for an unknown reason
            }
        }

        public bool IsDebugEnabled => Logger.IsEnabledFor(Level.Debug);

        public bool IsErrorEnabled => Logger.IsEnabledFor(Level.Error);

        public bool IsFatalEnabled => Logger.IsEnabledFor(Level.Fatal);

        public bool IsInfoEnabled => Logger.IsEnabledFor(Level.Info);

        public bool IsWarnEnabled => Logger.IsEnabledFor(Level.Warn);

        protected internal OrchardLog4netFactory Factory { get; set; }

        protected internal log4net.Core.ILogger Logger { get; set; }

        public override string ToString()
        {
            return Logger.ToString();
        }

        public virtual Logger CreateChildLogger(string name)
        {
            return Factory.Create(Logger.Name + "." + name);
        }

        public void Debug(string message)
        {
            if (IsDebugEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Debug, message, null);
            }
        }

        public void Debug(Func<string> messageFactory)
        {
            if (IsDebugEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Debug, messageFactory.Invoke(), null);
            }
        }

        public void Debug(string message, Exception exception)
        {
            if (IsDebugEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Debug, message, exception);
            }
        }

        public void DebugFormat(string format, params object[] args)
        {
            if (IsDebugEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Debug, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
            }
        }

        public void DebugFormat(Exception exception, string format, params object[] args)
        {
            if (IsDebugEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Debug, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
            }
        }

        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (IsDebugEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Debug, new SystemStringFormat(formatProvider, format, args), null);
            }
        }

        public void DebugFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (IsDebugEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Debug, new SystemStringFormat(formatProvider, format, args), exception);
            }
        }

        public void Error(string message)
        {
            if (IsErrorEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Error, message, null);
            }
        }

        public void Error(Func<string> messageFactory)
        {
            if (IsErrorEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Error, messageFactory.Invoke(), null);
            }
        }

        public void Error(string message, Exception exception)
        {
            if (IsErrorEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Error, message, exception);
            }
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (IsErrorEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Error, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
            }
        }

        public void ErrorFormat(Exception exception, string format, params object[] args)
        {
            if (IsErrorEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Error, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
            }
        }

        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (IsErrorEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Error, new SystemStringFormat(formatProvider, format, args), null);
            }
        }

        public void ErrorFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (IsErrorEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Error, new SystemStringFormat(formatProvider, format, args), exception);
            }
        }

        public void Fatal(string message)
        {
            if (IsFatalEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Fatal, message, null);
            }
        }

        public void Fatal(Func<string> messageFactory)
        {
            if (IsFatalEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Fatal, messageFactory.Invoke(), null);
            }
        }

        public void Fatal(string message, Exception exception)
        {
            if (IsFatalEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Fatal, message, exception);
            }
        }

        public void FatalFormat(string format, params object[] args)
        {
            if (IsFatalEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Fatal, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
            }
        }

        public void FatalFormat(Exception exception, string format, params object[] args)
        {
            if (IsFatalEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Fatal, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
            }
        }

        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (IsFatalEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Fatal, new SystemStringFormat(formatProvider, format, args), null);
            }
        }

        public void FatalFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (IsFatalEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Fatal, new SystemStringFormat(formatProvider, format, args), exception);
            }
        }

        public void Info(string message)
        {
            if (IsInfoEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Info, message, null);
            }
        }

        public void Info(Func<string> messageFactory)
        {
            if (IsInfoEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Info, messageFactory.Invoke(), null);
            }
        }

        public void Info(string message, Exception exception)
        {
            if (IsInfoEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Info, message, exception);
            }
        }

        public void InfoFormat(string format, params object[] args)
        {
            if (IsInfoEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Info, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
            }
        }

        public void InfoFormat(Exception exception, string format, params object[] args)
        {
            if (IsInfoEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Info, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
            }
        }

        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (IsInfoEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Info, new SystemStringFormat(formatProvider, format, args), null);
            }
        }

        public void InfoFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (IsInfoEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Info, new SystemStringFormat(formatProvider, format, args), exception);
            }
        }

        public void Warn(string message)
        {
            if (IsWarnEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Warn, message, null);
            }
        }

        public void Warn(Func<string> messageFactory)
        {
            if (IsWarnEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Warn, messageFactory.Invoke(), null);
            }
        }

        public void Warn(string message, Exception exception)
        {
            if (IsWarnEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Warn, message, exception);
            }
        }

        public void WarnFormat(string format, params object[] args)
        {
            if (IsWarnEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Warn, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
            }
        }

        public void WarnFormat(Exception exception, string format, params object[] args)
        {
            if (IsWarnEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Warn, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
            }
        }

        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (IsWarnEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Warn, new SystemStringFormat(formatProvider, format, args), null);
            }
        }

        public void WarnFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (IsWarnEnabled)
            {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Warn, new SystemStringFormat(formatProvider, format, args), exception);
            }
        }

        public override object InitializeLifetimeService()
        {
            // never expire the cross-AppDomain lease on this object
            return null;
        }
    }
}
