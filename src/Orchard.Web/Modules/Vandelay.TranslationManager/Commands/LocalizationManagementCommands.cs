using System;
using System.Web.Hosting;
using Fluent.IO;
using Orchard.Commands;
using Orchard.Environment.Extensions;
using Vandelay.TranslationManager.Services;

namespace Vandelay.TranslationManager.Commands {
    [OrchardFeature("Vandelay.TranslationManager")]
    public class LocalizationManagementCommands : DefaultOrchardCommandHandler {
        private readonly ILocalizationManagementService _localizationManagementService;

        public LocalizationManagementCommands(ILocalizationManagementService localizationManagementService) {
            _localizationManagementService = localizationManagementService;
        }

        [OrchardSwitch]
        public string Culture { get; set; }

        [OrchardSwitch]
        public string Extensions { get; set; }

        [OrchardSwitch]
        public string Input { get; set; }

        [OrchardSwitch]
        public string Output { get; set; }

        [CommandName("install translation")]
        [CommandHelp(@"install translation <translationZip>
    Installs the translation files contained in the zip file.
    Translations for modules that are not physically present in the application are skipped.")]
        public void InstallTranslationCommand(string translationZip) {
            var siteRoot = HostingEnvironment.ApplicationPhysicalPath;
            _localizationManagementService.InstallTranslation(Path.Get(translationZip).ReadBytes(), siteRoot);
        }

        [CommandName("package translation")]
        [CommandHelp(@"package translation /Culture:<cultureCode> /Extensions:<extensionName1>,<extensionName2>... /Input:<inputDirectory> /Output:<outputDirectory>
    Packages translation files for the specified culture into a zip file in the specified output directory.
    One or more module or theme names can be specified, in which case only the po files for those modules and themes are included in the package.
    If no extension name is specified, all modules are scanned for po files, as well as Core and App_Data")]
        [OrchardSwitches("Culture,Extensions,Input,Output")]
        public void PackageTranslations() {
            var culture = String.IsNullOrEmpty(Culture) ? "en-us" : Culture;
            var siteRoot = GetInputDir();
            var zipContents = String.IsNullOrEmpty(Extensions) ?
                _localizationManagementService.PackageTranslations(culture, siteRoot) :
                _localizationManagementService.PackageTranslations(culture, siteRoot, Extensions.Split(','));
            var outputPath = Path.Get(Output).Combine("Orchard." + culture + ".po.zip");
            outputPath.Write(zipContents);
        }

        [CommandName("extract default translation")]
        [CommandHelp(@"extract default translation /Extensions:<extensionName1>,<extensionName2>... /Input:<inputDirectory> /Output:<outputDirectory>
    Extracts and packages a translation file for the default culture")]
        [OrchardSwitches("Extensions,Input,Output")]
        public void ExtractDefaultTranslation() {
            var siteRoot = GetInputDir();
            var outputPath = Path.Get(Output).Combine("Orchard.en-us.po.zip");
            outputPath.Write(
                String.IsNullOrEmpty(Extensions) ?
                _localizationManagementService.ExtractDefaultTranslation(siteRoot) :
                _localizationManagementService.ExtractDefaultTranslation(siteRoot, Extensions.Split(',')));
        }

        [CommandName("sync translation")]
        [CommandHelp(@"sync translation /Input:<translationDirectory> /Culture:<cultureCode>
    Synchronizes the translation for the specified culture with the default translation.
    If the translation for the specified culture does not exist yet, this generates a stub for it.
    The Output switch must point to a directory that contains both translations.")]
        [OrchardSwitches("Culture,Input")]
        public void SyncTranslation() {
            string siteRoot = GetInputDir();
            _localizationManagementService.SyncTranslation(siteRoot, Culture);
        }

        private string GetInputDir() {
            return String.IsNullOrEmpty(Input) ?
                HostingEnvironment.ApplicationPhysicalPath :
                Path.Current.Combine(Input).FullPath;
        }
    }
}