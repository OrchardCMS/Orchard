using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuild.Orchard.Tasks {
    public class FilterModuleBinaries : Task {
        public ITaskItem[] ModulesBinaries { get; set; }
        public ITaskItem[] OrchardWebBinaries { get; set; }

        [Output]
        public ITaskItem[] ExcludedBinaries { get; set; }

        public override bool Execute() {
            if (ModulesBinaries == null || OrchardWebBinaries == null)
                return true;

            var orchardWebAssemblies = new HashSet<string>(
                OrchardWebBinaries.Select(item => Path.GetFileName(item.ItemSpec)),
                StringComparer.InvariantCultureIgnoreCase);

            ExcludedBinaries = ModulesBinaries
                .Where(item => orchardWebAssemblies.Contains(Path.GetFileName(item.ItemSpec)))
                .Select(item => new TaskItem(item))
                .ToArray();

            return true;
        }
    }
}
