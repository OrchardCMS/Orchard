using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Features {
    public interface IFeatureManager : IDependency {
        IEnumerable<FeatureDescriptor> GetAvailableFeatures();
        IEnumerable<FeatureDescriptor> GetEnabledFeatures();

        void EnableFeatures(IEnumerable<string> featureNames);
        void DisableFeatures(IEnumerable<string> featureNames);
    }

    public class FeatureManager : IFeatureManager {
        private readonly IExtensionManager _extensionManager;
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IShellDescriptorManager _shellDescriptorManager;

        public FeatureManager(
            IExtensionManager extensionManager,
            ShellDescriptor shellDescriptor,
            IShellDescriptorManager shellDescriptorManager) {
            _extensionManager = extensionManager;
            _shellDescriptor = shellDescriptor;
            _shellDescriptorManager = shellDescriptorManager;
        }

        public IEnumerable<FeatureDescriptor> GetAvailableFeatures() {
            return _extensionManager.AvailableFeatures();
        }

        public IEnumerable<FeatureDescriptor> GetEnabledFeatures() {
            var currentShellDescriptor = _shellDescriptorManager.GetShellDescriptor();
            return _extensionManager.EnabledFeatures(currentShellDescriptor);
        }

        public void EnableFeatures(IEnumerable<string> featureNames) {
            var currentShellDescriptor = _shellDescriptorManager.GetShellDescriptor();

            var updatedFeatures = currentShellDescriptor.Features
                .Union(featureNames
                           .Where(name => !currentShellDescriptor.Features.Any(sf => sf.Name == name))
                           .Select(name => new ShellFeature {Name = name}));            

            _shellDescriptorManager.UpdateShellDescriptor(
                currentShellDescriptor.SerialNumber,
                updatedFeatures,
                currentShellDescriptor.Parameters);
        }

        public void DisableFeatures(IEnumerable<string> featureNames) {
            var currentShellDescriptor = _shellDescriptorManager.GetShellDescriptor();

            var updatedFeatures = currentShellDescriptor.Features
                .Where(sf => !featureNames.Contains(sf.Name));

            _shellDescriptorManager.UpdateShellDescriptor(
                currentShellDescriptor.SerialNumber,
                updatedFeatures,
                currentShellDescriptor.Parameters);
        }


        //private void DisableThemeFeatures(string themeName) {
        //    var themes = new Queue<string>();
        //    while (themeName != null) {
        //        if (themes.Contains(themeName))
        //            throw new InvalidOperationException(T("The theme \"{0}\" is already in the stack of themes that need features disabled.", themeName).Text);
        //        var theme = GetThemeByName(themeName);
        //        if (theme == null)
        //            break;
        //        themes.Enqueue(themeName);

        //        themeName = !string.IsNullOrWhiteSpace(theme.BaseTheme)
        //            ? theme.BaseTheme
        //            : null;

        //    }

        //    while (themes.Count > 0)
        //        _moduleService.DisableFeatures(new[] { themes.Dequeue() });
        //}

        //private void EnableThemeFeatures(string themeName) {
        //    var themes = new Stack<string>();
        //    while (themeName != null) {
        //        if (themes.Contains(themeName))
        //            throw new InvalidOperationException(T("The theme \"{0}\" is already in the stack of themes that need features enabled.", themeName).Text);
        //        themes.Push(themeName);

        //        var theme = GetThemeByName(themeName);
        //        themeName = !string.IsNullOrWhiteSpace(theme.BaseTheme)
        //            ? theme.BaseTheme
        //            : null;
        //    }

        //    while (themes.Count > 0)
        //        _moduleService.EnableFeatures(new[] { themes.Pop() });
        //}

        //private bool DoEnableTheme(string themeName) {
        //    if (string.IsNullOrWhiteSpace(themeName))
        //        return false;

        //    //todo: (heskew) need messages given in addition to all of these early returns so something meaningful can be presented to the user
        //    var themeToEnable = GetThemeByName(themeName);
        //    if (themeToEnable == null)
        //        return false;

        //    // ensure all base themes down the line are present and accounted for
        //    //todo: (heskew) dito on the need of a meaningful message
        //    if (!AllBaseThemesAreInstalled(themeToEnable.BaseTheme))
        //        return false;

        //    // enable all theme features
        //    EnableThemeFeatures(themeToEnable.Name);
        //    return true;
        //}

    }
}
