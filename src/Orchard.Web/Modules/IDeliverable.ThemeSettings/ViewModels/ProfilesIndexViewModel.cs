using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using IDeliverable.ThemeSettings.Models;

namespace IDeliverable.ThemeSettings.ViewModels
{
    public class ProfilesIndexViewModel
    {
        public IList<ThemeProfile> Profiles { get; set; }
        public ExtensionDescriptor Theme { get; set; }
    }
}