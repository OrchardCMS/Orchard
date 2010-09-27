namespace Orchard.UI.Resources {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("jQuery").SetUrl("jquery-1.4.2.min.js", "jquery-1.4.2.js").SetVersion("1.4.2");

            // todo: include min versions
            manifest.DefineScript("jQueryUtils").SetUrl("jquery.utils.js").SetDependencies("jQuery");
            manifest.DefineScript("jQueryUI_Core").SetUrl("jquery.ui.core.js").SetVersion("1.8b1").SetDependencies("jQuery");
            manifest.DefineScript("jQueryUI_Widget").SetUrl("jquery.ui.widget.js").SetVersion("1.8b1").SetDependencies("jQuery");
            manifest.DefineScript("jQueryUI_DatePicker").SetUrl("jquery.ui.datepicker.js").SetVersion("1.8b1").SetDependencies("jQueryUI_Core", "jQueryUI_Widget");
            manifest.DefineScript("jQueryUtils_TimePicker").SetUrl("ui.timepickr.js").SetVersion("0.7.0a").SetDependencies("jQueryUtils", "jQueryUI_Core");

            manifest.DefineStyle("jQueryUtils_TimePicker").SetUrl("ui.timepickr.css");
            manifest.DefineStyle("jQueryUI_Orchard").SetUrl("jquery-ui-1.7.2.custom.css").SetVersion("1.7.2");
            manifest.DefineStyle("jQueryUI_DatePicker").SetUrl("ui.datepicker.css").SetDependencies("jQueryUI_Orchard").SetVersion("1.7.2");
        }
    }
}
