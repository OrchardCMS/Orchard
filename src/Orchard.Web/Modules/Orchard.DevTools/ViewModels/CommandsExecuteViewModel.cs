using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.DevTools.ViewModels {
    public class CommandsExecuteViewModel {
        public string[] History { get; set; }
        public string CommandLine { get; set; }
        public string Results { get; set; }
    }
}