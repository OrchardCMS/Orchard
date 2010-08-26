using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.DisplayManagement {
    public class DisplayContext  {
        public DisplayHelper Display { get; set; }
        public ViewContext ViewContext { get; set; }
        public object Value { get; set; }
    }
}
