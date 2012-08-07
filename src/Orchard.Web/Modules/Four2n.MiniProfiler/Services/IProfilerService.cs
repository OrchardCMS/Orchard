// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IProfilerService.cs" company="Daniel Dabrowski - rod.42n.pl">
//   Copyright (c) 2008 Daniel Dabrowski - 42n. All rights reserved.
// </copyright>
// <summary>
//   Defines the IProfilerService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Four2n.Orchard.MiniProfiler.Services
{
    using global::Orchard;

    public interface IProfilerService : IDependency
    {
        void StepStart(string key, string message, bool isVerbose = false);

        void StepStop(string key);
    }
}