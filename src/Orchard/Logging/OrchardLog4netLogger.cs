using System;
using System.Globalization;
using System.Web;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using log4net;
using log4net.Core;
using log4net.Util;

using Logger = Castle.Core.Logging.ILogger;

namespace Orchard.Logging {
    [Serializable]
    public class OrchardLog4netLogger : MarshalByRefObject, Logger, IShim {
        private static readonly Type declaringType = typeof(OrchardLog4netLogger);

        private readonly Lazy<ShellSettings> _shellSettings;

        public IOrchardHostContainer HostContainer { get; set; }

        public OrchardLog4netLogger(log4net.Core.ILogger logger, OrchardLog4netFactory factory) {
            OrchardHostContainerRegistry.RegisterShim(this);
            Logger = logger;
            Factory = factory;

             _shellSettings = new Lazy<ShellSettings>(LoadSettings, System.Threading.LazyThreadSafetyMode.PublicationOnly);
        }

        internal OrchardLog4netLogger() {
        }

        internal OrchardLog4netLogger(ILog log, OrchardLog4netFactory factory)
            : this(log.Logger, factory) {
        }

        private ShellSettings LoadSettings() {
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
        protected internal void AddExtendedThreadInfo() {
            if (_shellSettings.Value != null) {
                ThreadContext.Properties["Tenant"] = _shellSettings.Value.Name;
            }
            else {
                ThreadContext.Properties.Remove("Tenant");
            }

            try {
                var ctx = HttpContext.Current;
                if (ctx != null) {
                    ThreadContext.Properties["Url"] = ctx.Request.Url.ToString();
                }
                else {
                    ThreadContext.Properties.Remove("Url");
                }
            }
            catch(HttpException) {
                // can happen on cloud service for an unknown reason
            }
        }

        public bool IsDebugEnabled {
            get { return Logger.IsEnabledFor(Level.Debug); }
        }

        public bool IsErrorEnabled {
            get { return Logger.IsEnabledFor(Level.Error); }
        }

        public bool IsFatalEnabled {
            get { return Logger.IsEnabledFor(Level.Fatal); }
        }

        public bool IsInfoEnabled {
            get { return Logger.IsEnabledFor(Level.Info); }
        }

        public bool IsWarnEnabled {
            get { return Logger.IsEnabledFor(Level.Warn); }
        }

        protected internal OrchardLog4netFactory Factory { get; set; }

        protected internal log4net.Core.ILogger Logger { get; set; }

        public override string ToString() {
            return Logger.ToString();
        }

        public virtual Logger CreateChildLogger(String name) {
            return Factory.Create(Logger.Name + "." + name);
        }

        public void Debug(String message) {
            if (IsDebugEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Debug, message, null);
            }
        }

        public void Debug(Func<string> messageFactory) {
            if (IsDebugEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Debug, messageFactory.Invoke(), null);
            }
        }

        public void Debug(String message, Exception exception) {
            if (IsDebugEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Debug, message, exception);
            }
        }

        public void DebugFormat(String format, params Object[] args) {
            if (IsDebugEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Debug, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
            }
        }

        public void DebugFormat(Exception exception, String format, params Object[] args) {
            if (IsDebugEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Debug, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
            }
        }

        public void DebugFormat(IFormatProvider formatProvider, String format, params Object[] args) {
            if (IsDebugEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Debug, new SystemStringFormat(formatProvider, format, args), null);
            }
        }

        public void DebugFormat(Exception exception, IFormatProvider formatProvider, String format, params Object[] args) {
            if (IsDebugEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Debug, new SystemStringFormat(formatProvider, format, args), exception);
            }
        }

        public void Error(String message) {
            if (IsErrorEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Error, message, null);
            }
        }

        public void Error(Func<string> messageFactory) {
            if (IsErrorEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Error, messageFactory.Invoke(), null);
            }
        }

        public void Error(String message, Exception exception) {
            if (IsErrorEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Error, message, exception);
            }
        }

        public void ErrorFormat(String format, params Object[] args) {
            if (IsErrorEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Error, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
            }
        }

        public void ErrorFormat(Exception exception, String format, params Object[] args) {
            if (IsErrorEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Error, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
            }
        }

        public void ErrorFormat(IFormatProvider formatProvider, String format, params Object[] args) {
            if (IsErrorEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Error, new SystemStringFormat(formatProvider, format, args), null);
            }
        }

        public void ErrorFormat(Exception exception, IFormatProvider formatProvider, String format, params Object[] args) {
            if (IsErrorEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Error, new SystemStringFormat(formatProvider, format, args), exception);
            }
        }

        public void Fatal(String message) {
            if (IsFatalEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Fatal, message, null);
            }
        }

        public void Fatal(Func<string> messageFactory) {
            if (IsFatalEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Fatal, messageFactory.Invoke(), null);
            }
        }

        public void Fatal(String message, Exception exception) {
            if (IsFatalEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Fatal, message, exception);
            }
        }

        public void FatalFormat(String format, params Object[] args) {
            if (IsFatalEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Fatal, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
            }
        }

        public void FatalFormat(Exception exception, String format, params Object[] args) {
            if (IsFatalEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Fatal, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
            }
        }

        public void FatalFormat(IFormatProvider formatProvider, String format, params Object[] args) {
            if (IsFatalEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Fatal, new SystemStringFormat(formatProvider, format, args), null);
            }
        }

        public void FatalFormat(Exception exception, IFormatProvider formatProvider, String format, params Object[] args) {
            if (IsFatalEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Fatal, new SystemStringFormat(formatProvider, format, args), exception);
            }
        }

        public void Info(String message) {
            if (IsInfoEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Info, message, null);
            }
        }

        public void Info(Func<string> messageFactory) {
            if (IsInfoEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Info, messageFactory.Invoke(), null);
            }
        }

        public void Info(String message, Exception exception) {
            if (IsInfoEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Info, message, exception);
            }
        }

        public void InfoFormat(String format, params Object[] args) {
            if (IsInfoEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Info, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
            }
        }

        public void InfoFormat(Exception exception, String format, params Object[] args) {
            if (IsInfoEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Info, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
            }
        }

        public void InfoFormat(IFormatProvider formatProvider, String format, params Object[] args) {
            if (IsInfoEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Info, new SystemStringFormat(formatProvider, format, args), null);
            }
        }

        public void InfoFormat(Exception exception, IFormatProvider formatProvider, String format, params Object[] args) {
            if (IsInfoEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Info, new SystemStringFormat(formatProvider, format, args), exception);
            }
        }

        public void Warn(String message) {
            if (IsWarnEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Warn, message, null);
            }
        }

        public void Warn(Func<string> messageFactory) {
            if (IsWarnEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Warn, messageFactory.Invoke(), null);
            }
        }

        public void Warn(String message, Exception exception) {
            if (IsWarnEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Warn, message, exception);
            }
        }

        public void WarnFormat(String format, params Object[] args) {
            if (IsWarnEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Warn, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
            }
        }

        public void WarnFormat(Exception exception, String format, params Object[] args) {
            if (IsWarnEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Warn, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
            }
        }

        public void WarnFormat(IFormatProvider formatProvider, String format, params Object[] args) {
            if (IsWarnEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Warn, new SystemStringFormat(formatProvider, format, args), null);
            }
        }

        public void WarnFormat(Exception exception, IFormatProvider formatProvider, String format, params Object[] args) {
            if (IsWarnEnabled) {
                AddExtendedThreadInfo();
                Logger.Log(declaringType, Level.Warn, new SystemStringFormat(formatProvider, format, args), exception);
            }
        }

        [Obsolete("Use IsFatalEnabled instead")]
        public bool IsFatalErrorEnabled {
            get {
                return Logger.IsEnabledFor(Level.Fatal);
            }
        }

        [Obsolete("Use DebugFormat instead")]
        public void Debug(string format, params object[] args) {
            if (IsDebugEnabled) {
                Logger.Log(declaringType, Level.Debug, string.Format(format, args), null);
            }
        }

        [Obsolete("Use ErrorFormat instead")]
        public void Error(string format, params object[] args) {
            if (IsErrorEnabled) {
                Logger.Log(declaringType, Level.Error, string.Format(format, args), null);
            }
        }

        [Obsolete("Use FatalFormat instead")]
        public void Fatal(string format, params object[] args) {
            if (IsFatalEnabled) {
                Logger.Log(declaringType, Level.Fatal, string.Format(format, args), null);
            }
        }

        [Obsolete("Use Fatal instead")]
        public void FatalError(string message) {
            if (IsFatalErrorEnabled) {
                Logger.Log(declaringType, Level.Fatal, message, null);
            }
        }

        [Obsolete("Use FatalFormat instead")]
        public void FatalError(string format, params object[] args) {
            if (IsFatalErrorEnabled) {
                Logger.Log(declaringType, Level.Fatal, string.Format(format, args), null);
            }
        }

        [Obsolete("Use Fatal instead")]
        public void FatalError(string message, Exception exception) {
            if (IsFatalErrorEnabled) {
                Logger.Log(declaringType, Level.Fatal, message, exception);
            }
        }

        [Obsolete("Use InfoFormat instead")]
        public void Info(string format, params object[] args) {
            if (IsInfoEnabled) {
                Logger.Log(declaringType, Level.Info, string.Format(format, args), null);
            }
        }

        [Obsolete("Use WarnFormat instead")]
        public void Warn(string format, params object[] args) {
            if (IsWarnEnabled) {
                Logger.Log(declaringType, Level.Warn, string.Format(format, args), null);
            }
        }

        public override object InitializeLifetimeService() {
            // never expire the cross-AppDomain lease on this object
            return null;
        }
    }
}
