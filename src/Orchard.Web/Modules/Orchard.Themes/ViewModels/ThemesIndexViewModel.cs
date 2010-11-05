using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Themes.ViewModels {
    public class ThemesIndexViewModel  {
        public ExtensionDescriptor CurrentTheme { get; set; }
        public IEnumerable<ExtensionDescriptor> Themes { get; set; }
        public IEnumerable<string> FeaturesThatNeedUpdate { get; set; }
    }
}