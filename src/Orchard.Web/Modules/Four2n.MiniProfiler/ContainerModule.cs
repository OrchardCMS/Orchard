// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContainerModule.cs" company="Daniel Dabrowski - rod.42n.pl">
//   Copyright (c) 2008 Daniel Dabrowski - 42n. All rights reserved.
// </copyright>
// <summary>
//   Defines the ContainerModule type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Four2n.Orchard.MiniProfiler
{
    using System;

    using Autofac;

    using Four2n.Orchard.MiniProfiler.Data;

    using global::Orchard.Environment;

    using StackExchange.Profiling;
    using StackExchange.Profiling.Storage;

    using Module = Autofac.Module;

    public class ContainerModule : Module
    {
        private readonly IOrchardHost orchardHost;

        public ContainerModule(IOrchardHost orchardHost)
        {
            this.orchardHost = orchardHost;
        }

        protected override void Load(ContainerBuilder moduleBuilder)
        {
            InitProfilerSettings();
            var currentLogger = ((DefaultOrchardHost)this.orchardHost).Logger;
            if (currentLogger is OrchardHostProxyLogger)
            {
                return;
            }

            ((DefaultOrchardHost)this.orchardHost).Logger = new OrchardHostProxyLogger(currentLogger);
        }

        private static void InitProfilerSettings()
        {
            MiniProfiler.Settings.SqlFormatter = new PoorMansTSqlFormatter();
            MiniProfiler.Settings.Storage = new ProfilerStorage(TimeSpan.FromSeconds(30));
            MiniProfiler.Settings.StackMaxLength = 500;
            MiniProfiler.Settings.ExcludeAssembly("MiniProfiler");
            MiniProfiler.Settings.ExcludeAssembly("NHibernate");
            WebRequestProfilerProvider.Settings.UserProvider = new IpAddressIdentity();
        }
    }
}