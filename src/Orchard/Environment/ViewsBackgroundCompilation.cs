using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Web.Compilation;
using Orchard.FileSystems.VirtualPath;
using Orchard.Logging;
using Orchard.Exceptions;

namespace Orchard.Environment {
    public interface IViewsBackgroundCompilation {
        void Start();
        void Stop();
    }

    public class ViewsBackgroundCompilation : IViewsBackgroundCompilation {
        private readonly IVirtualPathProvider _virtualPathProvider;
        private volatile bool _stopping;

        public ViewsBackgroundCompilation(IVirtualPathProvider virtualPathProvider) {
            _virtualPathProvider = virtualPathProvider;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Start() {
            _stopping = false;
            var timer = new Timer();
            timer.Elapsed += CompileViews;
            timer.Interval = TimeSpan.FromMilliseconds(1500).TotalMilliseconds;
            timer.AutoReset = false;
            timer.Start();
        }

        public void Stop() {
            _stopping = true;
        }

        public class CompilationContext {
            public IEnumerable<string> DirectoriesToBrowse { get; set; }
            public IEnumerable<string> FileExtensionsToCompile { get; set; }
            public HashSet<string> ProcessedDirectories { get; set; }
        }

        private void CompileViews(object sender, ElapsedEventArgs elapsedEventArgs) {
            var totalTime = new Stopwatch();
            totalTime.Start();
            Logger.Information("Starting background compilation of views");
            ((Timer)sender).Stop();

            // Hard-coded context based on current orchard profile
            var context = new CompilationContext {
                // Put most frequently used directories first in the list
                DirectoriesToBrowse = new[] {
                    // Setup
                    "~/Modules/Orchard.Setup/Views",
                    "~/Themes/SafeMode/Views",

                    // Common
                    "~/Core/Contents/Views",
                    "~/Core/Common/Views",
                    "~/Core/Settings/Views",

                    // Dashboard
                    "~/Core/Dashboard/Views",
                    "~/Themes/TheAdmin/Views",

                    // Content Items
                    "~/Modules/Orchard.PublishLater/Views",

                    // Content Types
                    "~/Modules/Orchard.ContentTypes/Views",

                    // "Edit" homepage
                    "~/Modules/TinyMce/Views",
                    "~/Modules/Orchard.Tags/Views",
                    "~/Core/Navigation/Views",

                    // Various other admin pages
                    "~/Core/Settings/Views",
                    "~/Core/Containers/Views",
                    "~/Modules/Orchard.Widgets/Views",
                    "~/Modules/Orchard.Users/Views",
                    "~/Modules/Orchard.MediaLibrary/Views",
                    "~/Modules/Orchard.Comments/Views",

                    // Leave these at end (as a best effort)
                    "~/Core", "~/Modules", "~/Themes"
                },
                FileExtensionsToCompile = new[] { ".cshtml" },
                ProcessedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            };

            var directories = context
                .DirectoriesToBrowse
                .SelectMany(folder => GetViewDirectories(folder, context.FileExtensionsToCompile));

            int directoryCount = 0;
            foreach (var viewDirectory in directories) {
                if (_stopping) {
                    if (Logger.IsEnabled(LogLevel.Information)) {
                        var leftOvers = directories.Except(context.ProcessedDirectories).ToList();
                        Logger.Information("Background compilation stopped before all directories were processed ({0} directories left)", leftOvers.Count);
                        foreach (var directory in leftOvers) {
                            Logger.Information("Directory not processed: '{0}'", directory);
                        }
                    }
                    break;
                }

                CompileDirectory(context, viewDirectory);
                directoryCount++;
            }

            totalTime.Stop();
            Logger.Information("Ending background compilation of views, {0} directories processed in {1} sec", directoryCount, totalTime.Elapsed.TotalSeconds);
        }

        private void CompileDirectory(CompilationContext context, string viewDirectory) {
            // Prevent processing of the same directories multiple times (sligh performance optimization,
            // as the build manager second call to compile a view is essentially a "no-op".
            if (context.ProcessedDirectories.Contains(viewDirectory))
                return;
            context.ProcessedDirectories.Add(viewDirectory);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try {
                var firstFile = _virtualPathProvider
                    .ListFiles(viewDirectory)
                    .Where(f => context.FileExtensionsToCompile.Any(e => f.EndsWith(e, StringComparison.OrdinalIgnoreCase)))
                    .FirstOrDefault();

                if (firstFile != null)
                    BuildManager.GetCompiledAssembly(firstFile);
            }
            catch(Exception ex) {
                if (ex.IsFatal()) {
                    throw;
                } 
                // Some views might not compile, this is ok and harmless in this
                // context of pre-compiling views.
                Logger.Information(ex, "Compilation of directory '{0}' skipped", viewDirectory);
            }
            stopwatch.Stop();
            Logger.Information("Directory '{0}' compiled in {1} msec", viewDirectory, stopwatch.ElapsedMilliseconds);
        }

        private IEnumerable<string> GetViewDirectories(string directory, IEnumerable<string> extensions) {
            var result = new List<string>();
            GetViewDirectories(_virtualPathProvider, directory, extensions, result);
            return result;
        }

        private static void GetViewDirectories(IVirtualPathProvider vpp, string directory, IEnumerable<string> extensions, ICollection<string> files) {
            if (vpp.ListFiles(directory).Where(f => extensions.Any(e => f.EndsWith(e, StringComparison.OrdinalIgnoreCase))).Any()) {
                files.Add(directory);
            }

            foreach (var childDirectory in vpp.ListDirectories(directory).OrderBy(d => d, StringComparer.OrdinalIgnoreCase)) {
                GetViewDirectories(vpp, childDirectory, extensions, files);
            }
        }
    }
}