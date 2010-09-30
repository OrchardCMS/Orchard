using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using Orchard.Commands;
using Orchard.Data.Migration.Generator;
using Orchard.CodeGeneration.Services;
using Orchard.Data.Migration.Schema;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;

namespace Orchard.CodeGeneration.Commands {

    [OrchardFeature("Generate")]
    public class CodeGenerationCommands : DefaultOrchardCommandHandler {
        private readonly IExtensionManager _extensionManager;
        private readonly ISchemaCommandGenerator _schemaCommandGenerator;

        private const string ModuleName = "CodeGeneration";
        private static readonly string CodeGenTemplatePath = HostingEnvironment.MapPath("~/Modules/Orchard." + ModuleName + "/CodeGenerationTemplates/");

        public CodeGenerationCommands(
            IExtensionManager extensionManager,
            ISchemaCommandGenerator schemaCommandGenerator) {
            _extensionManager = extensionManager;
            _schemaCommandGenerator = schemaCommandGenerator;
        }

        [OrchardSwitch]
        public bool IncludeInSolution { get; set; }

        [OrchardSwitch]
        public string BasedOn { get; set; }

        [CommandHelp("generate create datamigration <feature-name> \r\n\t" + "Create a new Data Migration class")]
        [CommandName("generate create datamigration")]
        public void CreateDataMigration(string featureName) {
            Context.Output.WriteLine(T("Creating Data Migration for {0}", featureName));

            ExtensionDescriptor extensionDescriptor = _extensionManager.AvailableExtensions().FirstOrDefault(extension => extension.ExtensionType == "Module" &&
                                                                                                             extension.Features.Any(feature => String.Equals(feature.Name, featureName, StringComparison.OrdinalIgnoreCase)));

            if (extensionDescriptor == null) {
                Context.Output.WriteLine(T("Creating data migration failed: target Feature {0} could not be found.", featureName));
                return;
            }

            string dataMigrationsPath = HostingEnvironment.MapPath("~/Modules/" + extensionDescriptor.Name + "/DataMigrations/");
            string dataMigrationPath = dataMigrationsPath + extensionDescriptor.DisplayName + "DataMigration.cs";
            string templatesPath = HostingEnvironment.MapPath("~/Modules/Orchard." + ModuleName + "/CodeGenerationTemplates/");
            string moduleCsProjPath = HostingEnvironment.MapPath(string.Format("~/Modules/{0}/{0}.csproj", extensionDescriptor.Name));
                    
            if (!Directory.Exists(dataMigrationsPath)) {
                Directory.CreateDirectory(dataMigrationsPath);
            }

            if (File.Exists(dataMigrationPath)) {
                Context.Output.WriteLine(T("Data migration already exists in target Module {0}.", extensionDescriptor.Name));
                return;
            }

            List<SchemaCommand> commands = _schemaCommandGenerator.GetCreateFeatureCommands(featureName, false).ToList();
                    
            var stringWriter = new StringWriter();
            var interpreter = new CodeGenerationCommandInterpreter(stringWriter);

            foreach (var command in commands) {
                interpreter.Visit(command);
                stringWriter.WriteLine();
            }

            string dataMigrationText = File.ReadAllText(templatesPath + "DataMigration.txt");
            dataMigrationText = dataMigrationText.Replace("$$FeatureName$$", featureName);
            dataMigrationText = dataMigrationText.Replace("$$ClassName$$", extensionDescriptor.DisplayName);
            dataMigrationText = dataMigrationText.Replace("$$Commands$$", stringWriter.ToString());
            File.WriteAllText(dataMigrationPath, dataMigrationText);

            string projectFileText = File.ReadAllText(moduleCsProjPath);

            // The string searches in solution/project files can be made aware of comment lines.
            if ( projectFileText.Contains("<Compile Include") ) {
                string compileReference = string.Format("<Compile Include=\"{0}\" />\r\n    ", "DataMigrations\\" + extensionDescriptor.DisplayName + "DataMigration.cs");
                projectFileText = projectFileText.Insert(projectFileText.LastIndexOf("<Compile Include"), compileReference);
            }
            else {
                string itemGroupReference = string.Format("</ItemGroup>\r\n  <ItemGroup>\r\n    <Compile Include=\"{0}\" />\r\n  ", "DataMigrations\\" + extensionDescriptor.DisplayName + "DataMigration.cs");
                projectFileText = projectFileText.Insert(projectFileText.LastIndexOf("</ItemGroup>"), itemGroupReference);
            }

            File.WriteAllText(moduleCsProjPath, projectFileText);
            TouchSolution();
            Context.Output.WriteLine(T("Data migration created successfully in Module {0}", extensionDescriptor.Name));
        }

        [CommandHelp("generate create module <module-name> [/IncludeInSolution:true|false]\r\n\t" + "Create a new Orchard module")]
        [CommandName("generate create module")]
        [OrchardSwitches("IncludeInSolution")]
        public void CreateModule(string moduleName) {
            Context.Output.WriteLine(T("Creating Module {0}", moduleName));

            if ( _extensionManager.AvailableExtensions().Any(extension => String.Equals(moduleName, extension.DisplayName, StringComparison.OrdinalIgnoreCase)) ) {
                Context.Output.WriteLine(T("Creating Module {0} failed: a module of the same name already exists", moduleName));
                return;
            }

            IntegrateModule(moduleName);
            Context.Output.WriteLine(T("Module {0} created successfully", moduleName));
        }

        [CommandName("generate create theme")]
        [CommandHelp("generate create theme <theme-name> [/IncludeInSolution:true|false][/BasedOn:<theme-name>]\r\n\tCreate a new Orchard theme")]
        [OrchardSwitches("IncludeInSolution,BasedOn")]
        public void CreateTheme(string themeName) {
            Context.Output.WriteLine(T("Creating Theme {0}", themeName));
            if (_extensionManager.AvailableExtensions().Any(extension => String.Equals(themeName, extension.DisplayName, StringComparison.OrdinalIgnoreCase))) {
                Context.Output.WriteLine(base.T("Creating Theme {0} failed: an extention of the same name already exists", themeName));
            }
            else {
                string baseThemePath = null;
                if (!string.IsNullOrEmpty(BasedOn)) {
                    baseThemePath = HostingEnvironment.MapPath("~/Themes/" + BasedOn + "/");
                    if (string.IsNullOrEmpty(baseThemePath) || Directory.Exists(baseThemePath)) {
                        Context.Output.WriteLine(T("Creating Theme {0} failed: could not find base theme '{1}'", themeName, baseThemePath));
                    }
                }
                IntegrateTheme(themeName, baseThemePath);
                Context.Output.WriteLine(base.T("Theme {0} created successfully", new object[] {themeName}));
            }
        }

        [CommandHelp("generate create controller <module-name> <controller-name>\r\n\t" + "Create a new Orchard controller in a module")]
        [CommandName("generate create controller")]
        public void CreateController(string moduleName, string controllerName) {
            Context.Output.WriteLine(T("Creating Controller {0} in Module {1}", controllerName, moduleName));

            ExtensionDescriptor extensionDescriptor = _extensionManager.AvailableExtensions().FirstOrDefault(extension => extension.ExtensionType == "Module" &&
                                                                                                             string.Equals(moduleName, extension.DisplayName, StringComparison.OrdinalIgnoreCase));

            if (extensionDescriptor == null) {
                Context.Output.WriteLine(T("Creating Controller {0} failed: target Module {1} could not be found.", controllerName, moduleName));
                return;
            }

            string moduleControllersPath = HostingEnvironment.MapPath("~/Modules/" + extensionDescriptor.Name + "/Controllers/");
            string controllerPath = moduleControllersPath + controllerName + ".cs";
            string moduleCsProjPath = HostingEnvironment.MapPath(string.Format("~/Modules/{0}/{0}.csproj", extensionDescriptor.Name));
            string templatesPath = HostingEnvironment.MapPath("~/Modules/Orchard." + ModuleName + "/CodeGenerationTemplates/");

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
            TouchSolution();
        }

        private void IntegrateModule(string moduleName) {
            string rootWebProjectPath = HostingEnvironment.MapPath("~/Orchard.Web.csproj");
            string projectGuid = Guid.NewGuid().ToString().ToUpper();

            CreateFilesFromTemplates(moduleName, projectGuid);
            // The string searches in solution/project files can be made aware of comment lines.
            if (IncludeInSolution) {
                AddToSolution(moduleName, projectGuid, null, null);
            }
        }

        private void IntegrateTheme(string themeName, string baseThemePath) {
            HashSet<string> createdFiles;
            HashSet<string> createdFolders;
            var projectGuid = Guid.NewGuid().ToString().ToUpper();
            CreateThemeFromTemplates(themeName, baseThemePath, projectGuid, out createdFiles, out createdFolders);
            if (IncludeInSolution) {
                AddToSolution(themeName, null, createdFiles, createdFolders);
            }
        }

        private static void CreateFilesFromTemplates(string moduleName, string projectGuid) {
            string modulePath = HostingEnvironment.MapPath("~/Modules/" + moduleName + "/");
            string propertiesPath = modulePath + "Properties";

            Directory.CreateDirectory(modulePath);
            Directory.CreateDirectory(propertiesPath);
            Directory.CreateDirectory(modulePath + "Controllers");
            Directory.CreateDirectory(modulePath + "Views");
            File.WriteAllText(modulePath + "\\Views\\Web.config", File.ReadAllText(CodeGenTemplatePath + "ViewsWebConfig.txt"));
            Directory.CreateDirectory(modulePath + "Models");
            Directory.CreateDirectory(modulePath + "Scripts");

            string templateText = File.ReadAllText(CodeGenTemplatePath + "ModuleAssemblyInfo.txt");
            templateText = templateText.Replace("$$ModuleName$$", moduleName);
            templateText = templateText.Replace("$$ModuleTypeLibGuid$$", Guid.NewGuid().ToString());
            File.WriteAllText(propertiesPath + "\\AssemblyInfo.cs", templateText);
            File.WriteAllText(modulePath + "\\Web.config", File.ReadAllText(CodeGenTemplatePath + "ModuleWebConfig.txt"));
            templateText = File.ReadAllText(CodeGenTemplatePath + "ModuleManifest.txt");
            templateText = templateText.Replace("$$ModuleName$$", moduleName);
            File.WriteAllText(modulePath + "\\Module.txt", templateText);
            templateText = File.ReadAllText(CodeGenTemplatePath + "\\ModuleCsProj.txt");
            templateText = templateText.Replace("$$ModuleName$$", moduleName);
            templateText = templateText.Replace("$$ModuleProjectGuid$$", projectGuid);
            File.WriteAllText(modulePath + "\\" + moduleName + ".csproj", templateText);
        }

        private static void CreateThemeFromTemplates(string themeName, string baseThemePath, string projectGuid, out HashSet<string> createdFiles, out HashSet<string> createdFolders) {
            var themePath = HostingEnvironment.MapPath("~/Themes/" + themeName + "/");
            createdFiles = new HashSet<string>();
            createdFolders = new HashSet<string>();
            // create directories
            foreach (var folderName in new string[] { "", "Content", "Styles", "Scripts", "Views", "Zones" }) {
                var folder = themePath + folderName;
                createdFolders.Add(folder);
                Directory.CreateDirectory(folder);
            }
            if (baseThemePath != null) {
                // copy BasedOn theme file by file
                foreach (var file in Directory.GetFiles(baseThemePath, "*", SearchOption.AllDirectories)) {
                    var destPath = file.Replace(baseThemePath, themePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                    File.Copy(file, destPath);
                    createdFiles.Add(destPath);
                }
            }
            else {
                // non-BasedOn theme default files
                var webConfig = themePath + "Views\\Web.config";
                File.WriteAllText(webConfig, File.ReadAllText(CodeGenTemplatePath + "\\ViewsWebConfig.txt"));
                createdFiles.Add(webConfig);
            }
            var templateText = File.ReadAllText(CodeGenTemplatePath + "\\ThemeManifest.txt").Replace("$$ThemeName$$", themeName);
            File.WriteAllText(themePath + "Theme.txt", templateText);
            createdFiles.Add(themePath + "Theme.txt");
        }


        private void AddToSolution(string projectName, string projectGuid, HashSet<string> filesToAddToOrchardWeb, HashSet<string> foldersToAddToOrchardWeb) {
            var rootWebProjectPath = HostingEnvironment.MapPath("~/Orchard.Web.csproj");
            if (!string.IsNullOrEmpty(projectGuid)) {
                var solutionPath = Directory.GetParent(rootWebProjectPath).Parent.FullName + "\\Orchard.sln";
                if (File.Exists(solutionPath)) {
                    var projectReference = string.Format("EndProject\r\nProject(\"{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}\") = \"{0}\", \"Orchard.Web\\Modules\\{0}\\{0}.csproj\", \"{{{1}}}\"\r\n", projectName, projectGuid);
                    var projectConfiguationPlatforms = string.Format("GlobalSection(ProjectConfigurationPlatforms) = postSolution\r\n\t\t{{{0}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU\r\n\t\t{{{0}}}.Debug|Any CPU.Build.0 = Debug|Any CPU\r\n\t\t{{{0}}}.Release|Any CPU.ActiveCfg = Release|Any CPU\r\n\t\t{{{0}}}.Release|Any CPU.Build.0 = Release|Any CPU\r\n", projectGuid);
                    var solutionText = File.ReadAllText(solutionPath);
                    solutionText = solutionText.Insert(solutionText.LastIndexOf("EndProject\r\n"), projectReference).Replace("GlobalSection(ProjectConfigurationPlatforms) = postSolution\r\n", projectConfiguationPlatforms);
                    solutionText = solutionText.Insert(solutionText.LastIndexOf("EndGlobalSection"), "\t{" + projectGuid + "} = {E9C9F120-07BA-4DFB-B9C3-3AFB9D44C9D5}\r\n\t");
                    File.WriteAllText(solutionPath, solutionText);
                    TouchSolution();
                }
                else {
                    Context.Output.WriteLine(base.T("Warning: Solution file could not be found at {0}", solutionPath));
                }
            }
            AddFilesToOrchardWeb(filesToAddToOrchardWeb, foldersToAddToOrchardWeb);
        }

        private void AddFilesToOrchardWeb(HashSet<string> content, HashSet<string> folders) {
            if (content == null && folders == null) {
                return;
            }

            var orchardWebProj = HostingEnvironment.MapPath("~/Orchard.Web.csproj");
            if (!File.Exists(orchardWebProj)) {
                Context.Output.WriteLine(T("Warning: Orchard.Web project file could not be found at {0}", orchardWebProj));
            }
            else {
                var filesBaseDir = Path.GetDirectoryName(orchardWebProj) + "\\";
                var contentInclude = "";
                if (content != null && content.Count > 0) {
                    contentInclude = string.Join("\r\n",
                                                 from file in content
                                                 select "    <Content Include=\"" + file.Replace(filesBaseDir, "") + "\" />");
                }
                if (folders != null && folders.Count > 0) {
                    contentInclude += "\r\n" + string.Join("\r\n", from folder in folders
                                                                   select "    <Folder Include=\"" + folder.Replace(filesBaseDir, "") + "\" />");
                }
                var itemGroup = string.Format(CultureInfo.InvariantCulture, "<ItemGroup>\r\n{0}\r\n  </ItemGroup>\r\n  ", contentInclude);
                var projectText = File.ReadAllText(orchardWebProj);
                // find where the first ItemGroup is after any References
                var refIndex = projectText.LastIndexOf("<Reference Include");
                if (refIndex != -1) {
                    var firstItemGroupIndex = projectText.IndexOf("<ItemGroup>", refIndex);
                    if (firstItemGroupIndex != -1) {
                        projectText = projectText.Insert(firstItemGroupIndex, itemGroup);
                        File.WriteAllText(orchardWebProj, projectText);
                        return;
                    }
                }
                Context.Output.WriteLine(T("Warning: Unable to modify Orchard.Web project file at {0}", orchardWebProj));
            }
        }

        private void TouchSolution() {
            string rootWebProjectPath = HostingEnvironment.MapPath("~/Orchard.Web.csproj");
            string solutionPath = Directory.GetParent(rootWebProjectPath).Parent.FullName + "\\Orchard.sln";
            if (!File.Exists(solutionPath)) {
                Context.Output.WriteLine(T("Warning: Solution file could not be found at {0}", solutionPath));
                return;
            }

            try {
                File.SetLastWriteTime(solutionPath, DateTime.Now);
            }
            catch {
                Context.Output.WriteLine(T("An unexpected error occured while trying to refresh the Visual Studio solution. Please reload it."));
            }
        }
    }
}