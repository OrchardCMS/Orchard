// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrchardHostProxyLogger.cs" company="Daniel Dabrowski - rod.42n.pl">
//   Copyright (c) 2008 Daniel Dabrowski - 42n. All rights reserved.
// </copyright>
// <summary>
//   Defines the OrchardHostProxyLogger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Four2n.Orchard.MiniProfiler
{
    using System;

    using global::Orchard.Logging;

    using StackExchange.Profiling;

    public class OrchardHostProxyLogger : ILogger
    {
        private readonly ILogger logger;

        public OrchardHostProxyLogger(ILogger logger)
        {
            this.logger = logger;
        }

        public bool IsEnabled(LogLevel level)
        {
            return true;
        }

        public void Log(LogLevel level, Exception exception, string format, params object[] args)
        {
            if (level == LogLevel.Debug)
            {
                if ("BeginRequest".Equals(format))
                {
                    MiniProfiler.Start(ProfileLevel.Verbose);
                }
                else if ("EndRequest".Equals(format))
                {
                    MiniProfiler.Stop();
                }
            }

            if (this.logger.IsEnabled(level))
            {
                this.logger.Log(level, exception, format, args);
            }
        }
    }
}