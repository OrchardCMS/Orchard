using System.Web.Mvc;
using Orchard;
using IDeliverable.ThemeSettings.Models;

namespace IDeliverable.ThemeSettings.Services
{
    public interface ISettingsFormBuilder : IDependency
    {
        dynamic BuildForm(ThemeSettingsManifest manifest);
        dynamic BindForm(dynamic form, IValueProvider valueProvider);
    }
}