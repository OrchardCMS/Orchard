using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.Commands;
using Orchard.ContentManagement.MetaData;
using Orchard.ImportExport.Recipes.Builders;
using Orchard.ImportExport.Services;
using Orchard.Recipes.Models;
using Orchard.Recipes.Providers.Builders;
using Orchard.Recipes.Services;
using Orchard.Security;
using Orchard.Settings;

namespace Orchard.ImportExport.Commands {
    public class ImportExportCommands : DefaultOrchardCommandHandler {
        private readonly IImportExportService _importExportService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISiteService _siteService;
        private readonly IMembershipService _membershipService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IOrchardServices _orchardServices;

        public ImportExportCommands(
            IImportExportService importExportService,
            IContentDefinitionManager contentDefinitionManager,
            ISiteService siteService,
            IMembershipService membershipService,
            IAuthenticationService authenticationService, 
            IOrchardServices orchardServices) {

            _importExportService = importExportService;
            _contentDefinitionManager = contentDefinitionManager;
            _siteService = siteService;
            _membershipService = membershipService;
            _authenticationService = authenticationService;
            _orchardServices = orchardServices;
        }

        [OrchardSwitch]
        public string Filename { get; set; }
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
        [CommandHelp("import file /Filename:<path> \r\n\t" + "Imports the content of a file.")]
        [OrchardSwitches("Filename")]
        public void ImportFile() {

            if (String.IsNullOrEmpty(Filename)) {
                Context.Output.WriteLine(T("Invalid file path"));
                return;
            }

            if (!File.Exists(Filename)) {
                Context.Output.WriteLine(T("File not found."));
                return;
            }

            _importExportService.Import(File.ReadAllText(Filename));

            Context.Output.WriteLine(T("Import running..."));
        }

        [CommandName("export file")]
        [CommandHelp("export file [/Types:<type-name-1>, ... ,<type-name-n>] [/Metadata:true|false] [/Data:true|false] [/Version:Published|Draft|Latest] [/SiteSettings:true|false] [/Steps:<custom-step-1>, ... ,<custom-step-n>]\r\n\t" + "Create an export file according to the specified options.")]
        [OrchardSwitches("Types,Metadata,Data,Version,SiteSettings,Steps")]
        public void ExportFile() {
            // Impersonate the Site owner.
            var superUser = _siteService.GetSiteSettings().SuperUser;
            var owner = _membershipService.GetUser(superUser);
            _authenticationService.SetAuthenticatedUserForRequest(owner);

            var versionOption = VersionHistoryOptions.Published;

            if (!String.IsNullOrEmpty(Version) && !Enum.TryParse(Version, out versionOption)) {
                Context.Output.WriteLine(T("Invalid version option"));
                return;
            }

            var enteredTypes = (Types ?? String.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            var exportTypes = _contentDefinitionManager
                .ListTypeDefinitions()
                .Where(contentType => enteredTypes.Contains(contentType.Name))
                .Select(contentType => contentType.Name)
                .ToList();

            var enteredSteps = (Steps ?? String.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var recipeBuilderSteps = new List<IRecipeBuilderStep>();

            if (Metadata || Data) {
                var dataStep = _orchardServices.WorkContext.Resolve<ContentStep>();

                if(Data)
                    dataStep.DataContentTypes = exportTypes;

                if(Metadata)
                    dataStep.SchemaContentTypes = exportTypes;

                dataStep.VersionHistoryOptions = versionOption;
                recipeBuilderSteps.Add(dataStep);
            }

            if (SiteSettings) {
                var siteSettingsStep = _orchardServices.WorkContext.Resolve<SettingsStep>();
                recipeBuilderSteps.Add(siteSettingsStep);
            }

            if (enteredSteps.Any()) {
                var customStepsStep = _orchardServices.WorkContext.Resolve<CustomStepsStep>();
                recipeBuilderSteps.Add(customStepsStep);
            }

            Context.Output.WriteLine(T("Export starting..."));

            var exportFilePath = _importExportService.Export(recipeBuilderSteps);

            Context.Output.WriteLine(T("Export completed at {0}", exportFilePath));
        }
    }
}