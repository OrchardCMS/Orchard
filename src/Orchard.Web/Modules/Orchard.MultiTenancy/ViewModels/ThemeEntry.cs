using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.MultiTenancy.ViewModels {
    public class ThemeEntry {
        public bool Checked { get; set; }
        public string ThemeName { get; set; }
        public string ThemeId { get; set; }
    }
}