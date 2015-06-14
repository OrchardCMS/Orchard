using IDeliverable.Licensing.Orchard;
using IDeliverable.Slides.Models;

namespace IDeliverable.Slides.Drivers
{
    public class SlidesLicenseSettingsPartDriver : LicenseSettingsPartDriverBase<SlidesLicenseSettingsPart>
    {
        protected override string EditorShapeName => "Parts_Slides_License_Edit";
        protected override string EditorShapeTemplateName => "Parts.Slides.License";
    }
}