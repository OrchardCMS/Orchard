using Orchard.UI.Resources;

namespace Orchard.Resources.ResourceManifests
{
    public class jQueryCalendars : IResourceManifestProvider
    {
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            // jQuery Calendars.
            manifest.DefineScript("jQueryCalendars")
                .SetUrl("jQuery.Calendars/jquery.calendars.all.min.js", "jQuery.Calendars/jquery.calendars.all.js")
                .SetDependencies("jQueryPlugin")
                .SetVersion("2.2.0");

            // jQuery Calendars Picker.
            manifest.DefineScript("jQueryCalendars_Picker")
                .SetUrl("jQuery.Calendars/jquery.calendars.picker.full.min.js", "jQuery.Calendars/jquery.calendars.picker.full.js")
                .SetDependencies("jQueryCalendars")
                .SetVersion("2.2.0");

            manifest.DefineStyle("jQueryCalendars_Picker")
                .SetUrl("jQuery.Calendars/jquery.calendars.picker.full.min.css", "jQuery.Calendars/jquery.calendars.picker.full.css")
                .SetDependencies("jQueryUI_Orchard")
                .SetVersion("2.0.0"); // The styles are still at version 2.0.0 in the 2.2.0 release.
        }
    }
}
