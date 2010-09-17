using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.UI.Resources;

namespace Orchard.Core.PublishLater {
    public class PublishLaterResourceManifest : ResourceManifest {
        public PublishLaterResourceManifest() {
            DefineStyle("PublishLater_DatePicker").SetUrl("datetime.css");

            // todo: move into Orchard.jQuery module and also include min versions
            DefineStyle("jQueryUtils_TimePicker").SetUrl("ui.timepickr.css");
            DefineStyle("jQueryUI_Orchard").SetUrl("jquery-ui-1.7.2.custom.css").SetVersion("1.7.2");
            DefineStyle("jQueryUI_DatePicker").SetUrl("ui.datepicker.css").SetDependencies("jQueryUI_Orchard").SetVersion("1.7.2");

            DefineScript("jQueryUtils").SetUrl("jquery.utils.js").SetDependencies("jQuery");
            DefineScript("jQueryUI_Core").SetUrl("jquery.ui.core.js").SetVersion("1.8b1").SetDependencies("jQuery");
            DefineScript("jQueryUI_Widget").SetUrl("jquery.ui.widget.js").SetVersion("1.8b1").SetDependencies("jQuery");
            DefineScript("jQueryUI_DatePicker").SetUrl("jquery.ui.datepicker.js").SetVersion("1.8b1").SetDependencies("jQueryUI_Core", "jQueryUI_Widget");
            DefineScript("jQueryUtils_TimePicker").SetUrl("ui.timepickr.js").SetVersion("0.7.0a").SetDependencies("jQueryUtils", "jQueryUI_Core");
        }
    }
}
