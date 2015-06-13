using System.ComponentModel.DataAnnotations;
using Orchard.Environment.Extensions.Models;

namespace IDeliverable.ThemeSettings.ViewModels
{
    public class ProfileViewModel
    {
        [Required, MaxLength(128)]
        public string Name { get; set; }
        public string Description { get; set; }
        public ExtensionDescriptor Theme { get; set; }
        public dynamic SettingsForm { get; set; }
        public bool IsCurrent { get; set; }
    }
}