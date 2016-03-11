using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.ImportExport.ViewModels;
using Orchard.Mvc;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Setup.Services;
using Orchard.Tasks;
using Orchard.UI.Notify;

namespace Orchard.ImportExport.Providers.ImportActions {
    public class ExecuteRecipeAction : ImportAction {
        private readonly IOrchardServices _orchardServices;
        private readonly ISetupService _setupService;
        private readonly ShellSettings _shellSettings;
        private readonly IEnumerable<IRecipeExecutionStep> _recipeExecutionSteps;
        private readonly IRecipeParser _recipeParser;
        private readonly IRecipeExecutor _recipeExecutor;
        private readonly IDatabaseManager _databaseManager;
        private readonly ISweepGenerator _sweepGenerator;
        private readonly IRecipeStepQueue _recipeStepQueue;
        private readonly IRepository<RecipeStepResultRecord> _recipeStepResultRepository;

        public ExecuteRecipeAction(
            IOrchardServices orchardServices,
            ISetupService setupService,
            ShellSettings shellSettings,
            IEnumerable<IRecipeExecutionStep> recipeExecutionSteps, 
            IRecipeParser recipeParser, 
            IRecipeExecutor recipeExecutor, 
            IDatabaseManager databaseManager, 
            ISweepGenerator sweepGenerator, 
            IRecipeStepQueue recipeStepQueue, 
            IRepository<RecipeStepResultRecord> recipeStepResultRepository) {

            _orchardServices = orchardServices;
            _setupService = setupService;
            _shellSettings = shellSettings;
            _recipeExecutionSteps = recipeExecutionSteps;
            _recipeParser = recipeParser;
            _recipeExecutor = recipeExecutor;
            _databaseManager = databaseManager;
            _sweepGenerator = sweepGenerator;
            _recipeStepQueue = recipeStepQueue;
            _recipeStepResultRepository = recipeStepResultRepository;

            RecipeExecutionTimeout = 600;
        }

        public override string Name { get { return "ExecuteRecipe"; } }

        public XDocument RecipeDocument { get; set; }
        public bool ResetSite { get; set; }
        public string SuperUserPassword { get; set; }
        public int RecipeExecutionTimeout { get; set; }

        public override dynamic BuildEditor(dynamic shapeFactory) {
            return UpdateEditor(shapeFactory, null);
        }

        public override dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater) {
            var viewModel = new UploadRecipeViewModel {
                SuperUserName = _orchardServices.WorkContext.CurrentSite.SuperUser,
                RecipeExecutionSteps = _recipeExecutionSteps.Select(x => new RecipeExecutionStepViewModel {
                    Name = x.Name,
                    DisplayName = x.DisplayName,
                    Description = x.Description,
                    Editor = x.BuildEditor(shapeFactory)
                }).Where(x => x.Editor != null).ToList()
            };

            if (updater != null) {
                if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                    // Validate and read uploaded recipe file.
                    var request = _orchardServices.WorkContext.HttpContext.Request;
                    var file = request.Files["RecipeFile"];
                    var isValid = true;

                    ResetSite = viewModel.ResetSite;
                    SuperUserPassword = viewModel.SuperUserPassword;

                    if (file == null || file.ContentLength == 0) {
                        updater.AddModelError("RecipeFile", T("No recipe file selected."));
                        isValid = false;
                    }

                    if (ResetSite) {
                        if (String.IsNullOrWhiteSpace(viewModel.SuperUserPassword)) {
                            updater.AddModelError("SuperUserPassword", T("Please specify a new password for the super user."));
                            isValid = false;
                        }
                        else if (!String.Equals(viewModel.SuperUserPassword, viewModel.SuperUserPasswordConfirmation)) {
                            updater.AddModelError("SuperUserPassword", T("The passwords do not match."));
                            isValid = false;
                        }
                    }

                    var stepUpdater = new Updater(updater, secondHalf => String.Format("{0}.{1}", Prefix, secondHalf));

                    // Update the view model with non-roundtripped values.
                    viewModel.SuperUserName = _orchardServices.WorkContext.CurrentSite.SuperUser;
                    foreach (var stepViewModel in viewModel.RecipeExecutionSteps) {
                        var step = _recipeExecutionSteps.Single(x => x.Name == stepViewModel.Name);
                        stepViewModel.DisplayName = step.DisplayName;
                        stepViewModel.Description = step.Description;

                        // Update the step with posted values.
                        stepViewModel.Editor = step.UpdateEditor(shapeFactory, stepUpdater);
                    }

                    if (isValid) {
                        // Read recipe file.
                        RecipeDocument = XDocument.Parse(new StreamReader(file.InputStream).ReadToEnd());
                    }
                }
            }

            return shapeFactory.EditorTemplate(TemplateName: "ImportActions/ExecuteRecipe", Model: viewModel, Prefix: Prefix);
        }

        public override void Configure(ImportActionConfigurationContext context) {
            ResetSite = context.ConfigurationElement.Attr<bool>("ResetSite");
            SuperUserPassword = context.ConfigurationElement.Attr("SuperUserPassword");

            var executionStepsElement = context.ConfigurationElement.Element("Steps");
            if (executionStepsElement == null)
                return;

            foreach (var stepElement in executionStepsElement.Elements()) {
                var step = _recipeExecutionSteps.SingleOrDefault(x => x.Name == stepElement.Name.LocalName);

                if (step != null) {
                    var stepContext = new RecipeExecutionStepConfigurationContext(stepElement);
                    step.Configure(stepContext);
                }
            }
        }

        public override void Execute(ImportActionContext context) {
            var recipeDocument = context.RecipeDocument ?? RecipeDocument;
            if (recipeDocument == null)
                return;

            // Give each execution step a chance to augment the recipe step before it will be scheduled.
            PrepareRecipe(recipeDocument);

            // Sets the request timeout to a configurable amount of seconds to give enough time to execute custom recipes.
            if (_orchardServices.WorkContext.HttpContext != null) {
                _orchardServices.WorkContext.HttpContext.Server.ScriptTimeout = RecipeExecutionTimeout;
            }

            // Suspend background task execution.
            _sweepGenerator.Terminate();

            // Import or setup using the specified recipe.
            var executionId = ResetSite ? Setup(recipeDocument) : ExecuteRecipe(recipeDocument);

            if(executionId == null) {
                _orchardServices.Notifier.Warning(T("The recipe contained no steps. No work was scheduled."));
                _sweepGenerator.Activate();
                return;
            }

            // Resume background tasks once import/setup completes.
            var recipe = _recipeParser.ParseRecipe(recipeDocument);
            var activateSweepGeneratorStep = new RecipeStep(Guid.NewGuid().ToString("N"), recipe.Name, "ActivateSweepGenerator", new XElement("ActivateSweepGenerator"));
            _recipeStepQueue.Enqueue(executionId, activateSweepGeneratorStep);
            _recipeStepResultRepository.Create(new RecipeStepResultRecord {
                ExecutionId = executionId,
                RecipeName = recipe.Name,
                StepId = activateSweepGeneratorStep.Id,
                StepName = activateSweepGeneratorStep.Name
            });

            context.ExecutionId = executionId;
            context.RecipeDocument = recipeDocument;
        }

        private string Setup(XDocument recipeDocument) {
            // Prepare Setup.
            var setupContext = new SetupContext {
                Recipe = _recipeParser.ParseRecipe(recipeDocument),
                AdminPassword = SuperUserPassword,
                AdminUsername = _orchardServices.WorkContext.CurrentSite.SuperUser,
                DatabaseConnectionString = _shellSettings.DataConnectionString,
                DatabaseProvider = _shellSettings.DataProvider,
                DatabaseTablePrefix = _shellSettings.DataTablePrefix,
                SiteName = _orchardServices.WorkContext.CurrentSite.SiteName,
                EnabledFeatures = Enumerable.Empty<string>()
            };

            // Delete the tenant tables.
            DropTenantDatabaseTables();

            // Execute Setup.
            var executionId = _setupService.Setup(setupContext);

            return executionId;
        }

        private string ExecuteRecipe(XDocument recipeDocument) {
            var recipe = _recipeParser.ParseRecipe(recipeDocument);
            return _recipeExecutor.Execute(recipe);
        }

        private void PrepareRecipe(XDocument recipeDocument) {
            var query = 
                from stepElement in recipeDocument.Element("Orchard").Elements()
                let step = _recipeExecutionSteps.SingleOrDefault(x => x.Name == stepElement.Name.LocalName)
                where step != null
                select new { Step = step, StepElement = stepElement };

            foreach (var step in query) {
                var context = new UpdateRecipeExecutionStepContext { Step = step.StepElement };
                step.Step.UpdateStep(context);
            }
        }

        private void DropTenantDatabaseTables() {
            _databaseManager.DropTenantDatabaseTables();
            _orchardServices.TransactionManager.RequireNew();
        }
    }
}