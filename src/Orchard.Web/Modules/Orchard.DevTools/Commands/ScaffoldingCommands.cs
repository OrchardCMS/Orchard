using System;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Orchard.Commands;
using Orchard.Data.Migration.Generator;
using Orchard.DevTools.Services;
using Orchard.Environment.Extensions;

namespace Orchard.DevTools.Commands {
    [OrchardFeature("Scaffolding")]
    public class ScaffoldingCommands : DefaultOrchardCommandHandler {
        private readonly IExtensionManager _extensionManager;
        private readonly ISchemaCommandGenerator _schemaCommandGenerator;

        public ScaffoldingCommands(IExtensionManager extensionManager,
            ISchemaCommandGenerator schemaCommandGenerator) {
            _extensionManager = extensionManager;
            _schemaCommandGenerator = schemaCommandGenerator;
        }

        [OrchardSwitch]
        public bool IncludeInSolution { get; set; }

        [CommandHelp("scaffolding create datamigration <feature-name> \r\n\t" + "Create a new Data Migration class")]
        [CommandName("scaffolding create datamigration")]
        public void CreateDataMigration(string featureName) {
            Context.Output.WriteLine(T("Creating Data Migration for {0}", featureName));

            foreach ( var extension in _extensionManager.AvailableExtensions() ) {
                if ( extension.ExtensionType == "Module" && extension.Features.Any(f => String.Equals(f.Name, featureName, StringComparison.OrdinalIgnoreCase)) ) {
                    string dataMigrationsPath = HostingEnvironment.MapPath("~/Modules/" + extension.Name + "/DataMigrations/");
                    string dataMigrationPath = dataMigrationsPath  + extension.DisplayName + "DataMigration.cs";
                    string templatesPath = HostingEnvironment.MapPath("~/Modules/Orchard.DevTools/ScaffoldingTemplates/");
                    if ( !Directory.Exists(dataMigrationsPath) ) {
                        Directory.CreateDirectory(dataMigrationsPath);
                    }
                    if ( File.Exists(dataMigrationPath) ) {
                        Context.Output.WriteLine(T("Data migration already exists in target Module {0}.", extension.Name));
                        return;
                    }

                    var commands = _schemaCommandGenerator.GetCreateFeatureCommands(featureName, false).ToList();
                    
                    var stringWriter = new StringWriter();
                    var interpreter = new ScaffoldingCommandInterpreter(stringWriter);

                    foreach ( var command in commands ) {
                        interpreter.Visit(command);
                        stringWriter.WriteLine();
                    }

                    string dataMigrationText = File.ReadAllText(templatesPath + "DataMigration.txt");
                    dataMigrationText = dataMigrationText.Replace("$$FeatureName$$", featureName);
                    dataMigrationText = dataMigrationText.Replace("$$ClassName$$", extension.DisplayName);
                    dataMigrationText = dataMigrationText.Replace("$$Commands$$", stringWriter.ToString());
                    File.WriteAllText(dataMigrationPath, dataMigrationText);
                    Context.Output.WriteLine(T("Data migration created successfully in Module {0}", extension.DisplayName));
                    return;
                }
            }
            Context.Output.WriteLine(T("Creating data migration failed: target Feature {0} could not be found.", featureName));
        }

        [CommandHelp("scaffolding create module <module-name> [/IncludeInSolution:true|false]\r\n\t" + "Create a new Orchard module")]
        [CommandName("scaffolding create module")]
        [OrchardSwitches("IncludeInSolution")]
        public void CreateModule(string moduleName) {
            Context.Output.WriteLine(T("Creating Module {0}", moduleName));

            if ( _extensionManager.AvailableExtensions().Any(extension => extension.ExtensionType == "Module" && String.Equals(moduleName, extension.DisplayName, StringComparison.OrdinalIgnoreCase)) ) {
                Context.Output.WriteLine(T("Creating Module {0} failed: a module of the same name already exists", moduleName));
                return;
            }

            IntegrateModule(moduleName);

            Context.Output.WriteLine(T("Module {0} created successfully", moduleName));
        }


        [CommandHelp("scaffolding create controller <module-name> <controller-name>\r\n\t" + "Create a new Orchard controller in a module")]
        [CommandName("scaffolding create controller")]
        public void CreateController(string moduleName, string controllerName) {
            Context.Output.WriteLine(T("Creating Controller {0} in Module {1}", controllerName, moduleName));

            foreach (var extension in _extensionManager.AvailableExtensions()) {
                if (extension.ExtensionType == "Module" && String.Equals(moduleName, extension.DisplayName, StringComparison.OrdinalIgnoreCase)) {
                    string moduleControllersPath = HostingEnvironment.MapPath("~/Modules/" + extension.Name + "/Controllers/");
                    string controllerPath = moduleControllersPath + controllerName + ".cs";
                    string moduleCsProjPath = HostingEnvironment.MapPath(string.Format("~/Modules/{0}/{0}.csproj", extension.Name)); 
                    string templatesPath = HostingEnvironment.MapPath("~/Modules/Orchard.DevTools/ScaffoldingTemplates/");
                    if (!Directory.Exists(moduleControllersPath)) {
                        Directory.CreateDirectory(moduleControllersPath);
                    }
                    if (File.Exists(controllerPath)) {
                        Context.Output.WriteLine(T("Controller {0} already exists in target Module {1}.", controllerName, moduleName));
                        return;
                    }
                    string controllerText = File.ReadAllText(templatesPath + "Controller.txt");
                    controllerText = controllerText.Replace("$$ModuleName$$", moduleName);
                    controllerText = controllerText.Replace("$$ControllerName$$", controllerName);
                    File.WriteAllText(controllerPath, controllerText);
                    string projectFileText = File.ReadAllText(moduleCsProjPath);
                    // The string searches in solution/project files can be made aware of comment lines.
                    if (projectFileText.Contains("<Compile Include")) {
                        string compileReference = string.Format("<Compile Include=\"{0}\" />\r\n    ", "Controllers\\" + controllerName + ".cs");
                        projectFileText = projectFileText.Insert(projectFileText.LastIndexOf("<Compile Include"), compileReference);
                    }
                    else {
                        string itemGroupReference = string.Format("</ItemGroup>\r\n  <ItemGroup>\r\n    <Compile Include=\"{0}\" />\r\n  ", "Controllers\\" + controllerName + ".cs");
                        projectFileText = projectFileText.Insert(projectFileText.LastIndexOf("</ItemGroup>"), itemGroupReference);
                    }
                    File.WriteAllText(moduleCsProjPath, projectFileText);
                    Context.Output.WriteLine(T("Controller {0} created successfully in Module {1}", controllerName, moduleName));
                    return;
                }
            }
            Context.Output.WriteLine(T("Creating Controller {0} failed: target Module {1} could not be found.", controllerName, moduleName));
        }

        private void IntegrateModule(string moduleName) {
            string rootWebProjectPath = HostingEnvironment.MapPath("~/Orchard.Web.csproj");
            string projectGuid = Guid.NewGuid().ToString().ToUpper();

            CreateFilesFromTemplates(moduleName, projectGuid);
            // The string searches in solution/project files can be made aware of comment lines.
            if (IncludeInSolution) {
                // Add project reference to Orchard.Web.csproj
                string webProjectReference = string.Format(
                    "</ProjectReference>\r\n    <ProjectReference Include=\"Modules\\Orchard.{0}\\Orchard.{0}.csproj\">\r\n      <Project>{{{1}}}</Project>\r\n      <Name>Orchard.{0}</Name>\r\n    ",
                    moduleName, projectGuid);
                string webProjectText = File.ReadAllText(rootWebProjectPath);
                webProjectText = webProjectText.Insert(webProjectText.LastIndexOf("</ProjectReference>\r\n"), webProjectReference);
                File.WriteAllText(rootWebProjectPath, webProjectText);

                // Add project to Orchard.sln
                string solutionPath = Directory.GetParent(rootWebProjectPath).Parent.FullName + "\\Orchard.sln";
                if (File.Exists(solutionPath)) {
                    string projectReference = string.Format(
                        "EndProject\r\nProject(\"{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}\") = \"Orchard.{0}\", \"Orchard.Web\\Modules\\Orchard.{0}\\Orchard.{0}.csproj\", \"{{{1}}}\"\r\n",
                        moduleName, projectGuid);
                    string projectConfiguationPlatforms = string.Format(
                        "GlobalSection(ProjectConfigurationPlatforms) = postSolution\r\n\t\t{{{0}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU\r\n\t\t{{{0}}}.Debug|Any CPU.Build.0 = Debug|Any CPU\r\n\t\t{{{0}}}.Release|Any CPU.ActiveCfg = Release|Any CPU\r\n\t\t{{{0}}}.Release|Any CPU.Build.0 = Release|Any CPU\r\n",
                        projectGuid);
                    string solutionText = File.ReadAllText(solutionPath);
                    solutionText = solutionText.Insert(solutionText.LastIndexOf("EndProject\r\n"), projectReference);
                    solutionText = solutionText.Replace("GlobalSection(ProjectConfigurationPlatforms) = postSolution\r\n", projectConfiguationPlatforms);
                    solutionText = solutionText.Insert(solutionText.LastIndexOf("EndGlobalSection"), "\t{" + projectGuid + "} = {E9C9F120-07BA-4DFB-B9C3-3AFB9D44C9D5}\r\n\t");

                    File.WriteAllText(solutionPath, solutionText);
                }
                else {
                    Context.Output.WriteLine(T("Warning: Solution file could not be found at {0}", solutionPath));
                }
            }
        }

        private static void CreateFilesFromTemplates(string moduleName, string projectGuid) {
            string modulePath = HostingEnvironment.MapPath("~/Modules/Orchard." + moduleName + "/");
            string propertiesPath = modulePath + "Properties";
            string templatesPath = HostingEnvironment.MapPath("~/Modules/Orchard.DevTools/ScaffoldingTemplates/");

            Directory.CreateDirectory(modulePath);
            Directory.CreateDirectory(propertiesPath);
            string templateText = File.ReadAllText(templatesPath + "ModuleAssemblyInfo.txt");
            templateText = templateText.Replace("$$ModuleName$$", moduleName);
            templateText = templateText.Replace("$$ModuleTypeLibGuid$$", Guid.NewGuid().ToString());
            File.WriteAllText(propertiesPath + "\\AssemblyInfo.cs", templateText);
            File.WriteAllText(modulePath + "\\Web.config", File.ReadAllText(templatesPath + "ModuleWebConfig.txt"));
            templateText = File.ReadAllText(templatesPath + "ModuleManifest.txt");
            templateText = templateText.Replace("$$ModuleName$$", moduleName);
            File.WriteAllText(modulePath + "\\Module.txt", templateText);
            templateText = File.ReadAllText(templatesPath + "\\ModuleCsProj.txt");
            templateText = templateText.Replace("$$ModuleName$$", moduleName);
            templateText = templateText.Replace("$$ModuleProjectGuid$$", projectGuid);
            File.WriteAllText(modulePath + "\\Orchard." + moduleName + ".csproj", templateText);
        }
    }
}

