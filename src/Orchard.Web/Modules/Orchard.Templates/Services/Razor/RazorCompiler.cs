using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web.Razor;
using Microsoft.CSharp;
using Orchard.Caching;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Utility.Extensions;

namespace Orchard.Templates.Compilation.Razor {
    [OrchardFeature("Orchard.Templates.Razor")]
    public class RazorCompiler : IRazorCompiler {
        private readonly ICacheManager _cache;
        private readonly ISignals _signals;
        private const string DynamicallyGeneratedClassName = "RazorTemplate";
        private const string NamespaceForDynamicClasses = "Orchard.Compilation.Razor";
        private const string ForceRecompile = "Razor.ForceRecompile";
        private static readonly string[] DefaultNamespaces = {
                    "System",
                    "System.IO",
                    "System.Linq",
                    "System.Collections",
                    "System.Collections.Generic",
                    "System.Dynamic",
                    "System.Text",
                    "System.Web",
                    "System.Web.Mvc",
                    "System.Web.Mvc.Html",
                    "System.Web.Mvc.Ajax",
                    "System.Web.UI",
                    "System.Web.Routing",
                    "Orchard.Templates.Compilation.Razor",
                    "Orchard.ContentManagement",
                    "Orchard.DisplayManagement",
                    "Orchard.DisplayManagement.Shapes",
                    "Orchard.Security.Permissions",
                    "Orchard.UI.Resources",
                    "Orchard.Security",
                    "Orchard.Mvc.Spooling",
                    "Orchard.Mvc.Html"
                };

        public RazorCompiler(
            ICacheManager cache,
            ISignals signals) {
            _cache = cache;
            _signals = signals;
            Logger = NullLogger.Instance;
        }

        private ILogger Logger { get; set; }

        public IRazorTemplateBase<TModel> CompileRazor<TModel>(string code, string name, IDictionary<string, object> parameters) {
            return (RazorTemplateBase<TModel>)Compile(code, name, typeof(TModel), parameters);
        }

        public IRazorTemplateBase CompileRazor(string code, string name, IDictionary<string, object> parameters) {
            return (IRazorTemplateBase)Compile(code, name, null, parameters);
        }

        private object Compile(string code, string name, Type modelType, IDictionary<string, object> parameters) {

            var cacheKey = (name ?? DynamicallyGeneratedClassName) + GetHash(code);
            var generatedClassName = name != null ? name.Strip(c => !c.IsLetter() && !Char.IsDigit(c)) : DynamicallyGeneratedClassName;

            var assembly = _cache.Get(cacheKey, ctx => {
                _signals.When(ForceRecompile);

                var modelTypeName = "dynamic";
                var reader = new StringReader(code);
                var builder = new StringBuilder();

                // A hack to remove any @model directive as it's MVC-specific and compiler does not recognize it.
                // We should use this information to compile a strongly-typed template in the future
                string line;
                while ((line = reader.ReadLine()) != null) {
                    var trimmedLine = line.TrimStart(' ', '\t', '\n', '\r');
                    if (trimmedLine.StartsWith("@model ")) {
                        modelTypeName = trimmedLine.Substring("@model ".Length).Trim();
                        continue;
                    }

                    builder.AppendLine(line);
                }

                var language = new CSharpRazorCodeLanguage();
                var host = new RazorEngineHost(language) {
                    DefaultBaseClass = "RazorTemplateBase<" + modelTypeName + ">",
                    DefaultClassName = generatedClassName,
                    DefaultNamespace = NamespaceForDynamicClasses
                };

                foreach (var n in DefaultNamespaces) {
                    host.NamespaceImports.Add(n);
                }

                var engine = new RazorTemplateEngine(host);
                var razorTemplate = engine.GenerateCode(new StringReader(builder.ToString()));
                var compiledAssembly = CreateCompiledAssemblyFor(razorTemplate.GeneratedCode, name);
                return compiledAssembly;
            });

            return assembly.CreateInstance(NamespaceForDynamicClasses + "." + generatedClassName);
        }

        public static string GetHash(string value) {
            var data = Encoding.ASCII.GetBytes(value);
            var hashData = new MD5CryptoServiceProvider().ComputeHash(data);

            var strBuilder = new StringBuilder();
            hashData.Aggregate(strBuilder, (current, b) => strBuilder.Append(b));

            return strBuilder.ToString();
        }

        private static Assembly CreateCompiledAssemblyFor(CodeCompileUnit unitToCompile, string templateName) {
            var compilerParameters = new CompilerParameters();
            compilerParameters.ReferencedAssemblies.AddRange(AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => !a.IsDynamic)
                .Select(a => a.Location)
                .ToArray());

            compilerParameters.GenerateInMemory = true;

            var compilerResults = new CSharpCodeProvider().CompileAssemblyFromDom(compilerParameters, unitToCompile);
            if (compilerResults.Errors.HasErrors) {
                var errors = compilerResults.Errors.Cast<CompilerError>().Aggregate(string.Empty, (s, error) => s + "\r\nTemplate '" + templateName + "': " + error.ToString());
                throw new Exception(string.Format("Razor template compilation errors:\r\n{0}", errors));
            }
            else {
                var compiledAssembly = compilerResults.CompiledAssembly;
                return compiledAssembly;
            }
        }
    }
}