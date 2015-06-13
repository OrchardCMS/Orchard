using System.Collections.Generic;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Services;

namespace IDeliverable.ThemeSettings.ImportExport
{
    [OrchardFeature("IDeliverable.ThemeSettings.ImportExport")]
    public class ThemeSettingsExportStep : ICustomExportStep
    {
        public void Register(IList<string> steps)
        {
            steps.Add("ThemeSettings");
        }
    }
}