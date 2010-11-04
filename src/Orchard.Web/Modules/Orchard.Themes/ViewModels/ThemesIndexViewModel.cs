using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Themes.ViewModels {
    public class ThemesIndexViewModel  {
        public FeatureDescriptor CurrentTheme { get; set; }
        public IEnumerable<FeatureDescriptor> Themes { get; set; }
        public IEnumerable<string> FeaturesThatNeedUpdate { get; set; }
    }
}