using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Core.Common.Models;

namespace Orchard.Core.Common.ViewModels {
    public class BodyEditorViewModel {
        public BodyAspect BodyAspect { get; set; }

        public string Text {
            get { return BodyAspect.Record.Text; }
            set { BodyAspect.Record.Text = value; }
        }

        public string Format {
            get { return BodyAspect.Record.Format; }
            set { BodyAspect.Record.Format = value; }
        }

        public string TextEditorTemplate { get; set; }
    }
}
