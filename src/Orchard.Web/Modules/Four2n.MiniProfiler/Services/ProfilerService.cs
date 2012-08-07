// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProfilerService.cs" company="Daniel Dabrowski - rod.42n.pl">
//   Copyright (c) 2008 Daniel Dabrowski - 42n. All rights reserved.
// </copyright>
// <summary>
//   Defines the ProfilerService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Four2n.Orchard.MiniProfiler.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;

    using StackExchange.Profiling;

    public class ProfilerService : IProfilerService, IDisposable
    {
        private readonly ConcurrentDictionary<string, ConcurrentStack<IDisposable>> steps = new ConcurrentDictionary<string, ConcurrentStack<IDisposable>>();

        private MiniProfiler profiler;

        public ProfilerService()
        {
            this.profiler = MiniProfiler.Current;
        }

        protected MiniProfiler Profiler
        {
            get
            {
                // The event bus starts in a different scope where there's no MiniProfiler.Current, set it now
                return this.profiler ?? (this.profiler = MiniProfiler.Current);
            }
        }

        public void StepStart(string key, string message, bool isVerbose = false)
        {
            if (this.Profiler == null)
            {
                return;
            }

            var stack = this.steps.GetOrAdd(key, k => new ConcurrentStack<IDisposable>());
            var step = this.Profiler.Step(message, isVerbose ? ProfileLevel.Verbose : ProfileLevel.Info);
            stack.Push(step);
        }

        public void StepStop(string key)
        {
            if (this.Profiler == null)
            {
                return;
            }

            IDisposable step;
            if (this.steps[key].TryPop(out step))
            {
                step.Dispose();
            }
        }

        public void StopAll()
        {
            // Dispose any orphaned steps
            foreach (var stack in this.steps.Values)
            {
                IDisposable step;
                while (stack.TryPop(out step))
                {
                    step.Dispose();
                    Debug.WriteLine("[Four2n.MiniProfiler] - ProfilerService - StopAll There is some left");
                }
            }
        }

        public void Dispose()
        {
            this.StopAll();
        }
    }
}