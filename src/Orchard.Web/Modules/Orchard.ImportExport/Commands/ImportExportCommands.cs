using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Orchard.Commands;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.Recipes.Models;
using Orchard.Security;
using Orchard.Settings;

namespace Orchard.ImportExport.Commands {
    public class ImportExportCommands : DefaultOrchardCommandHandler {
        private readonly IImportExportService _importExportService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISiteService _siteService;
        private readonly IMembershipService _membershipService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IEnumerable<IExportAction> _exportActions;

        public ImportExportCommands(
            IImportExportService importExportService,
            IContentDefinitionManager contentDefinitionManager,
            ISiteService siteService,
            IMembershipService membershipService,
            IAuthenticationService authenticationService, 
            IEnumerable<IExportAction> exportActions) {

            _importExportService = importExportService;
            _contentDefinitionManager = contentDefinitionManager;
            _siteService = siteService;
            _membershipService = membershipService;
            _authenticationService = authenticationService;
            _exportActions = exportActions;
        }

        [OrchardSwitch]
        public string Filename { get; set; }
        [OrchardSwitch]
        public string ConfigFilename { get; set; }
        [OrchardSwitch]
        public string Types { get; set; }
        [OrchardSwitch]
        public bool Metadata { get; set; }
        [OrchardSwitch]
        public bool Data { get; set; }
        [OrchardSwitch]
        public string Steps { get; set; }
        [OrchardSwitch]
        public string Version { get; set; }
        [OrchardSwitch]
        public bool SiteSettings { get; set; }
        
        [CommandName("import file")]
        [CommandHelp("import file /Filename:<path> [/ConfigFilename:<configFilename>]\r\n\t" + "Imports the content of a file.")]
        [OrchardSwitches("Filename,ConfigFilename")]
        public void ImportFile() {
            if (String.IsNullOrEmpty(Filename)) {
                Context.Output.WriteLine(T("Invalid file path"));
                return;
            }

            if (!File.Exists(Filename)) {
                Context.Output.WriteLine(T("File not found."));
                return;
            }

            // Impersonate the Site owner.
            ImpersonateSuperUser();

            // Read config file if specified.
            var configurationDocument = ReadImportConfigurationFile(ConfigFilename);
            
            // Configure any steps based on the configuration.
            _importExportService.ConfigureImportActions(new ConfigureImportActionsContext(configurationDocument));

            // Import the file.
            _importExportService.Import(File.ReadAllText(Filename));

            Context.Output.WriteLine(T("Import running..."));
        }

        [CommandName("export file")]
        [CommandHelp("export file [/Filename:<path>] [/ConfigFilename:<path>] [/Types:<type-name-1>, ... ,<type-name-n>] [/Metadata:true|false] [/Data:true|false] [/Version:Published|Draft|Latest] [/SiteSettings:true|false] [/Steps:<custom-step-1>, ... ,<custom-step-n>]\r\n\t" + "Create an export file according to the specified options.")]
        [OrchardSwitches("Filename,ConfigFilename,Types,Metadata,Data,Version,SiteSettings,Steps")]
        public void ExportFile() {
            // Impersonate the Site owner.
            ImpersonateSuperUser();

            IEnumerable<IExportAction> actions;

            if (!IsAnySwitchDefined("ConfigFilename", "Types", "Metadata", "Version", "SiteSettings", "Steps")) {
                // Get default configured actions.
                actions = GetDefaultConfiguration();
            }
            else {
                // Read config file if specified.
                var configurationDocument = UpdateExportConfiguration(ReadExportConfigurationFile(ConfigFilename), Types, Metadata, Data, Version, SiteSettings, Steps);

                // Get all the steps based on the configuration.
                actions = _importExportService.ParseExportActions(configurationDocument);
            }

            Context.Output.WriteLine(T("Export starting..."));
            var exportContext = new ExportActionContext();
            _importExportService.Export(exportContext, actions);
            var exportFilePath = _importExportService.WriteExportFile(exportContext.RecipeDocument);

            if (!String.IsNullOrEmpty(Filename)) {
                var directory = Path.GetDirectoryName(Filename);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                File.Copy(exportFilePath, Filename, overwrite: true);
                exportFilePath = Filename;
            }

            Context.Output.WriteLine(T("Export completed at {0}", exportFilePath));
        }

        private bool IsAnySwitchDefined(params string[] switches) {
            return Context.Switches.Keys.Any(switches.Contains);
        }

        private IEnumerable<IExportAction> GetDefaultConfiguration() {
            foreach (var action in _exportActions) {
                action.ConfigureDefault();
            }

            return _exportActions;
        }

        private XDocument UpdateExportConfiguration(XDocument configurationDocument, string types, bool metadata, bool data, string version, bool siteSettings, string customSteps) {
            var buildRecipeElement = GetOrCreateElement(configurationDocument.Root, "BuildRecipe");
            var stepsElement = GetOrCreateElement(buildRecipeElement, "Steps");

            if (metadata || data) {
                var contentStepElement = GetOrCreateElement(stepsElement, "Content");
                var enteredTypes = (types ?? String.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                var exportTypes = _contentDefinitionManager
                    .ListTypeDefinitions()
                    .Where(contentType => enteredTypes.Contains(contentType.Name))
                    .Select(contentType => contentType.Name)
                    .ToList();

                if (data)
                    contentStepElement.Attr("DataContentTypes", String.Join(",", exportTypes));

                if (metadata)
                    contentStepElement.Attr("SchemaContentTypes", String.Join(",", exportTypes));

                if (!String.IsNullOrEmpty(version)) {
                    VersionHistoryOptions versionHistoryOptions;
                    if (Enum.TryParse(version, true, out versionHistoryOptions)) {
                        contentStepElement.Attr("VersionHistoryOptions", versionHistoryOptions);
                    }
                }
            }

            if (siteSettings) {
                GetOrCreateElement(stepsElement, "Settings");
            }

            if (!String.IsNullOrEmpty(customSteps)) {
                var customStepsList = customSteps.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var customStepName in customStepsList) {
                    GetOrCreateElement(stepsElement, customStepName);
                }

                //Still need CustomStepsStep to support older export steps created by users
                var customStepsElement = GetOrCreateElement(stepsElement, "CustomSteps");
                customStepsElement.Attr("Steps", customSteps);
            }

            return configurationDocument;
        }

        private void ImpersonateSuperUser() {
            var superUser = _siteService.GetSiteSettings().SuperUser;
            var owner = _membershipService.GetUser(superUser);
            _authenticationService.SetAuthenticatedUserForRequest(owner);
        }

        private XDocument ReadExportConfigurationFile(string filePath) {
            return !String.IsNullOrEmpty(filePath) && File.Exists(filePath) ? XDocument.Load(filePath) : new XDocument(new XElement("Export"));
        }

        private XDocument ReadImportConfigurationFile(string filePath) {
            return !String.IsNullOrEmpty(filePath) && File.Exists(filePath) ? XDocument.Load(filePath) : new XDocument(new XElement("Import"));
        }

        private XElement GetOrCreateElement(XElement element, string childElementName) {
            var childElement = element.Element(childElementName);

            if (childElement == null) {
                childElement = new XElement(childElementName);
                element.Add(childElement);
            }

            return childElement;
        }
    }
}