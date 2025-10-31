using Orchard.UI.Resources;

namespace Orchard.Resources.ResourceManifests
{
    public class jQueryTimeEntry : IResourceManifestProvider
    {
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest.DefineScript("jQueryTimeEntry")
                .SetUrl("jQuery.TimeEntry/jquery.timeentry.min.js", "jQuery.TimeEntry/jquery.timeentry.js")
                .SetDependencies("jQueryPlugin")
                .SetVersion("2.0.1");

            manifest.DefineStyle("jQueryTimeEntry")
                .SetUrl("jQuery.TimeEntry/jquery.timeentry.min.css", "jQuery.TimeEntry/jquery.timeentry.css")
                .SetVersion("2.0.1");
        }
    }
}
