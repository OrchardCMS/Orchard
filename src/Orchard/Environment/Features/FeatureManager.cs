using System;
using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Features {
    public interface IFeatureManager : IDependency {
        IEnumerable<FeatureInfo> GetFeatures();
        IEnumerable<FeatureDescriptor> GetAvailableFeatures();
        IEnumerable<FeatureDescriptor> GetEnabledFeatures();

        void EnableFeature(string name);
        void DisableFeature(string name);
    }

    public class FeatureInfo {
        ExtensionDescriptor Extension { get; set; }
        FeatureDescriptor Descriptor { get; set; }
        bool IsEnabled { get; set; }
    }

    public class FeatureManager : IFeatureManager {
        public IEnumerable<FeatureDescriptor> GetAvailableFeatures() {
            throw new NotImplementedException();
        }

        public IEnumerable<FeatureDescriptor> GetEnabledFeatures() {
            throw new NotImplementedException();
        }

        public void EnableFeature(string name) {
            throw new NotImplementedException();
        }

        public void DisableFeature(string name) {
            throw new NotImplementedException();
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
