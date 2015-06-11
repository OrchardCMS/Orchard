using System;
using System.Linq;
using Orchard.Commands;
using Orchard.Data.Migration;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Themes.Services;
using Orchard.Themes.Models;

namespace Orchard.Themes.Commands {
    public class ThemeCommands : DefaultOrchardCommandHandler     {
        private readonly IDataMigrationManager _dataMigrationManager;
        private readonly ISiteThemeService _siteThemeService;
        private readonly IExtensionManager _extensionManager;
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IThemeService _themeService;

        public ThemeCommands(IDataMigrationManager dataMigrationManager,
                             ISiteThemeService siteThemeService,
                             IExtensionManager extensionManager,
                             ShellDescriptor shellDescriptor,
                             IThemeService themeService) {
            _dataMigrationManager = dataMigrationManager;
            _siteThemeService = siteThemeService;
            _extensionManager = extensionManager;
            _shellDescriptor = shellDescriptor;
            _themeService = themeService;
        }

        [OrchardSwitch]
        public bool Summary { get; set; }

        [CommandName("theme list")]
        [CommandHelp("theme list [/Summary:true|false]" + "\r\n\tDisplay list of available themes")]
        [OrchardSwitches("Summary")]
        public void List() {
            var currentTheme = _siteThemeService.GetSiteTheme();
            var featuresThatNeedUpdate = _dataMigrationManager.GetFeaturesThatNeedUpdate();

            var themes = _extensionManager.AvailableExtensions()
                .Where(d => DefaultExtensionTypes.IsTheme(d.ExtensionType))
                .Where(d => d.Tags!= null && d.Tags.Split(',').Any(t => t.Trim().Equals("hidden", StringComparison.OrdinalIgnoreCase)) == false)
                .Select(d => new ThemeEntry {
                    Descriptor = d,
                    NeedsUpdate = featuresThatNeedUpdate.Contains(d.Id),
                    Enabled = _shellDescriptor.Features.Any(sf => sf.Name == d.Id)
                })
                .ToArray();

            if (Summary) {
                foreach (var theme in themes) {
                    Context.Output.WriteLine(T("{0}", theme.Name));
                }
            }
            else {
                Context.Output.WriteLine(T("Current theme"));
                Context.Output.WriteLine(T("--------------------------"));
                WriteThemeLines(new ThemeEntry {
                    Descriptor = currentTheme,
                    NeedsUpdate = featuresThatNeedUpdate.Contains(currentTheme.Id),
                    Enabled = _shellDescriptor.Features.Any(sf => sf.Name == currentTheme.Id)
                });

                Context.Output.WriteLine(T("List of available themes"));
                Context.Output.WriteLine(T("--------------------------"));
                themes.Where(t => t.Name.Trim().Equals(currentTheme.Name.Trim(), StringComparison.OrdinalIgnoreCase) == false)
                    .ToList()
                    .ForEach(WriteThemeLines);
            }
        }

        private void WriteThemeLines(ThemeEntry theme) {
            Context.Output.WriteLine(T("  Name: {0}", theme.Name));
            Context.Output.WriteLine(T("    State:         {0}", theme.Enabled ? T("Enabled") : T("Disabled")));
            Context.Output.WriteLine(T("    NeedsUpdate:   {0}", theme.NeedsUpdate ? T("Yes") : T("No")));
            Context.Output.WriteLine(T("    Author:        {0}", theme.Descriptor.Author));
            Context.Output.WriteLine(T("    Version:       {0}", theme.Descriptor.Version));
            Context.Output.WriteLine(T("    Description:   {0}", theme.Descriptor.Description));
            Context.Output.WriteLine(T("    Website:       {0}", theme.Descriptor.WebSite));
            Context.Output.WriteLine(T("    Zones:         {0}", string.Join(", ", theme.Descriptor.Zones)));
            Context.Output.WriteLine();
        }

        [CommandName("theme activate")]
        [CommandHelp("theme activate <theme-name>" + "\r\n\tEnable and activates a theme")]
        public void Activate(string themeName) {
            var theme = _extensionManager.AvailableExtensions().FirstOrDefault(x => x.Name.Trim().Equals(themeName, StringComparison.OrdinalIgnoreCase));
            if (theme == null) {
                Context.Output.WriteLine(T("Could not find theme {0}", themeName));
                return;
            }

            if (!_shellDescriptor.Features.Any(sf => sf.Name == theme.Id)) {
                Context.Output.WriteLine(T("Enabling theme \"{0}\"...", themeName));
                _themeService.EnableThemeFeatures(theme.Id);
            }

            Context.Output.WriteLine(T("Activating theme \"{0}\"...", themeName));
            _siteThemeService.SetSiteTheme(theme.Id);

            Context.Output.WriteLine(T("Theme \"{0}\" activated", themeName));
        }
    }
}
