using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Fluent.Zip;
using Orchard;
using Vandelay.TranslationManager.Models;
using Path = Fluent.IO.Path;

namespace Vandelay.TranslationManager.Services {
    public interface ILocalizationManagementService : IDependency {
        void InstallTranslation(byte[] zippedTranslation, string sitePath);
        byte[] PackageTranslations(string cultureCode, string sitePath);
        byte[] PackageTranslations(string cultureCode, string sitePath, IEnumerable<string> extensionNames);
        byte[] ExtractDefaultTranslation(string sitePath);
        byte[] ExtractDefaultTranslation(string sitePath, IEnumerable<string> extensionNames);
        void SyncTranslation(string sitePath, string cultureCode);
    }

    public class LocalizationManagementService : ILocalizationManagementService {
        private static readonly Regex ResourceStringExpression =
            new Regex(
                @"T\(((@"".*"")|(""([^""\\]|\\.)*?""))([^)""]*)\)",
                RegexOptions.Multiline | RegexOptions.Compiled);

        private static readonly Regex PluralStringExpression =
            new Regex(
                @"T.Plural\(((@"".*"")|(""([^""\\]|\\.)*?""))([^)""]*),\s*((@"".*"")|(""([^""\\]|\\.)*?""))([^)""]*)\)",
                RegexOptions.Multiline | RegexOptions.Compiled);

        private static readonly Regex NamespaceExpression =
            new Regex(
                @"namespace ([^\s]*)\s*{",
                RegexOptions.Multiline | RegexOptions.Compiled);

        private static readonly Regex ClassExpression =
            new Regex(
                @"\s?class ([^\s]*)\s",
                RegexOptions.Multiline | RegexOptions.Compiled);

        public void InstallTranslation(byte[] zippedTranslation, string sitePath) {
            var siteRoot = Path.Get(sitePath);
            ZipExtensions.Unzip(zippedTranslation, 
                (path, contents) => {
                    if (path.Extension == ".po") {
                        var tokens = path.Tokens;
                        var destPath = siteRoot.Combine(tokens);
                        // If a translation file is for a module or a theme, only install it
                        // if said module or theme exists already. Otherwise, skip.
                        if ((!tokens[0].Equals("modules", StringComparison.OrdinalIgnoreCase) &&
                             !tokens[0].Equals("themes", StringComparison.OrdinalIgnoreCase)) ||
                              siteRoot.Combine(tokens[0], tokens[1]).IsDirectory) {
                            destPath.Write(contents);
                        }
                    }
                });
        }

        public byte[] PackageTranslations(string cultureCode, string sitePath) {
            var site = Path.Get(sitePath);
            var translationFiles = site
                .Files("orchard.*.po", true)
                .Where(p => p.Parent().FileName.Equals(cultureCode, StringComparison.OrdinalIgnoreCase))
                .MakeRelativeTo(site);
            return ZipExtensions.Zip(
                translationFiles,
                p => site.Combine(p).ReadBytes());
        }

        public byte[] PackageTranslations(string cultureCode, string sitePath, IEnumerable<string> extensionNames) {
            var site = Path.Get(sitePath);
            var translationFiles = site
                .Files("orchard.*.po", true)
                .Where(p =>
                    p.Parent().FileName.Equals(cultureCode, StringComparison.OrdinalIgnoreCase) &&
                    (extensionNames.Contains(p.MakeRelativeTo(site.Combine("Modules")).Tokens[0]) ||
                     extensionNames.Contains(p.MakeRelativeTo(site.Combine("Themes")).Tokens[0])))
                .MakeRelativeTo(site);
            return ZipExtensions.Zip(
                translationFiles,
                p => site.Combine(p).ReadBytes());
        }

        public byte[] ExtractDefaultTranslation(string sitePath, IEnumerable<string> extensionNames) {
            if (extensionNames == null || extensionNames.Count() == 0) {
                return ExtractDefaultTranslation(sitePath);
            }
            var site = Path.Get(sitePath);
            var zipFiles = new Dictionary<Path, StringBuilder>();
            // Extract resources for module manifests
            site.Files("module.txt", true)
                .Where(p => extensionNames.Contains(p.Parent().FileName))
                .Read((content, path) => {
                    var moduleName = path.Parent().FileName;
                    var poPath = GetModuleLocalizationPath(site, moduleName);
                    ExtractPoFromManifest(zipFiles, poPath, content, path, site);
                });
            // Extract resources for theme manifests
            site.Files("theme.txt", true)
                .Where(p => extensionNames.Contains(p.Parent().FileName))
                .Read((content, path) => {
                    var themeName = path.Parent().FileName;
                    var poPath = GetThemeLocalizationPath(site, themeName);
                    ExtractPoFromManifest(zipFiles, poPath, content, path, site);
                });
            // Extract resources from views and cs files
            site.Files("*", true)
                .WhereExtensionIs(".cshtml", ".aspx", ".ascx", ".cs")
                .Where(p => {
                           var tokens = p.MakeRelativeTo(site).Tokens;
                           return new[] {"themes", "modules"}.Contains(tokens[0]) &&
                           extensionNames.Contains(tokens[1]);
                       })
                .Grep(
                    ResourceStringExpression,
                    (path, match, contents) => {
                        var str = match.Groups[1].ToString();
                        DispatchResourceString(zipFiles, null, null, site, path, site, contents, str);
                    }
                )
                .Grep(
                    PluralStringExpression,
                    (path, match, contents) => {
                        var str = match.Groups[1].ToString();
                        DispatchResourceString(zipFiles, null, null, site, path, site, contents, str);
                        str = match.Groups[6].ToString();
                        DispatchResourceString(zipFiles, null, null, site, path, site, contents, str);
                    }
                );
            return ZipExtensions.Zip(
                new Path(zipFiles.Keys.Select(p => p.MakeRelativeTo(site))),
                p => Encoding.UTF8.GetBytes(zipFiles[site.Combine(p)].ToString()));
        }

        public byte[] ExtractDefaultTranslation(string sitePath) {
            var site = Path.Get(sitePath);
            var corePoPath = site.Combine(
                "Core", "App_Data",
                "Localization", "en-US",
                "orchard.core.po");
            var rootPoPath = site.Combine(
                "App_Data", "Localization", "en-US",
                "orchard.root.po");
            var zipFiles = new Dictionary<Path, StringBuilder>();
            // Extract resources for module manifests
            site.Files("module.txt", true)
                .Read((content, path) => {
                    var moduleName = path.Parent().FileName;
                    var poPath =
                        path.MakeRelativeTo(sitePath).Tokens[0].Equals("core", StringComparison.OrdinalIgnoreCase) ?
                             corePoPath : GetModuleLocalizationPath(site, moduleName);
                    ExtractPoFromManifest(zipFiles, poPath, content, path, site);
                });
            // Extract resources for theme manifests
            site.Files("theme.txt", true)
                .Read((content, path) => {
                    var themeName = path.Parent().FileName;
                    var poPath = GetThemeLocalizationPath(site, themeName);
                    ExtractPoFromManifest(zipFiles, poPath, content, path, site);
                });
            // Extract resources from views and cs files, for the web site
            // as well as for the framework and Azure projects.
            site.Add(site.Parent().Combine("Orchard"))
                .Add(site.Parent().Combine("Orchard.Azure"))
                .ForEach(p =>
                    p.Files("*", true)
                    .WhereExtensionIs(".cshtml", ".aspx", ".ascx", ".cs")
                    .Grep(
                        ResourceStringExpression,
                        (path, match, contents) => {
                            var str = match.Groups[1].ToString();
                            DispatchResourceString(zipFiles, corePoPath, rootPoPath, site, path, p, contents, str);
                        }
                    )
                    .Grep(
                        PluralStringExpression,
                        (path, match, contents) => {
                            var str = match.Groups[1].ToString();
                            DispatchResourceString(zipFiles, corePoPath, rootPoPath, site, path, p, contents, str);
                            str = match.Groups[6].ToString();
                            DispatchResourceString(zipFiles, corePoPath, rootPoPath, site, path, p, contents, str);
                        }
                    ));
            return ZipExtensions.Zip(
                new Path(zipFiles.Keys.Select(p => p.MakeRelativeTo(site))),
                p => Encoding.UTF8.GetBytes(zipFiles[site.Combine(p)].ToString()));
        }

        public void SyncTranslation(string sitePath, string cultureCode) {
            Path.Get(sitePath)
                .Files("orchard.*.po", true)
                .Where(p => p.Parent().FileName.Equals("en-US", StringComparison.OrdinalIgnoreCase))
                .ForEach(baselinePath => {
                             var path = baselinePath.Parent().Parent().Combine(cultureCode, baselinePath.FileName);
                             var translations = new List<StringEntry>();
                             if (path.Exists) {
                                 path.Open(inStream => ReadTranslations(inStream, translations), FileMode.Open, FileAccess.Read, FileShare.Read);
                             }
                             path.Parent().CreateDirectory();
                             path.Open(outStream => {
                                           var englishTranslations = new List<StringEntry>();
                                           baselinePath.Open(baselineStream => ReadTranslations(baselineStream, englishTranslations), FileMode.Open, FileAccess.Read, FileShare.Read);
                                           using (var writer = new StreamWriter(outStream)) {
                                               foreach (var englishTranslation in englishTranslations) {
                                                   var entry = englishTranslation;
                                                   var translation = translations.Where(
                                                       t => t.Context == entry.Context &&
                                                            t.Key == entry.Key).FirstOrDefault();
                                                   if (translation == default(StringEntry) ||
                                                       translation.Translation == null ||
                                                       translation.Translation.Equals(@"msgstr """"")) {

                                                       writer.WriteLine("# Untranslated string");
                                                   }
                                                   writer.WriteLine(entry.Context);
                                                   writer.WriteLine(entry.Key);
                                                   writer.WriteLine(entry.English);
                                                   if (translation != null) {
                                                       translation.Used = true;
                                                       writer.WriteLine(translation.Translation);
                                                   }
                                                   else {
                                                       writer.WriteLine("msgstr \"\"");
                                                   }
                                                   writer.WriteLine();
                                               }
                                               foreach (var translation in translations.Where(t => !t.Used)) {
                                                   writer.WriteLine("# Obsolete translation");
                                                   writer.WriteLine(translation.Context);
                                                   writer.WriteLine(translation.Key);
                                                   writer.WriteLine(translation.English);
                                                   writer.WriteLine(translation.Translation);
                                                   writer.WriteLine();
                                               }
                                           }
                                       }, FileMode.Create, FileAccess.Write, FileShare.None);
                         });
        }

        private static void ReadTranslations(FileStream inStream, List<StringEntry> translations) {
            var translation = new StringEntry();
            var comparer = new StringEntryEqualityComparer();
            using (var reader = new StreamReader(inStream)) {
                while (!reader.EndOfStream) {
                    var line = reader.ReadLine();
                    if (line != null) {
                        if (line.StartsWith("#: ")) {
                            translation.Context = line;
                        }
                        else if (line.StartsWith("#| msgid ")) {
                            translation.Key = line;
                        }
                        else if (line.StartsWith("msgid ")) {
                            translation.English = line;
                        }
                        else if (line.StartsWith("msgstr ")) {
                            translation.Translation = line;
                            if (!translations.Contains(translation, comparer)) {
                                translations.Add(translation);
                            }
                            translation = new StringEntry();
                        }
                    }
                }
            }
        }

        private static StringBuilder GetBuilder(IDictionary<Path, StringBuilder> fileCatalog, Path path) {
            StringBuilder entry;
            if (!fileCatalog.ContainsKey(path)) {
                entry = new StringBuilder();
                fileCatalog.Add(path, entry);
            }
            else {
                entry = fileCatalog[path];
            }
            return entry;
        }

        private static void DispatchResourceString(
            IDictionary<Path, StringBuilder> fileCatalog,
            Path corePoPath,
            Path rootPoPath,
            Path sitePath,
            Path path,
            Path currentInputPath,
            string contents, string str) {

            var current = "~/" + path.MakeRelativeTo(currentInputPath).ToString().Replace('\\', '/');
            var context = current;
            if (path.Extension == ".cs") {
                var ns = NamespaceExpression.Match(contents).Groups[1].ToString();
                var type = ClassExpression.Match(contents).Groups[1].ToString();
                context = ns + "." + type;
            }
            Path targetPath;
            if (current.StartsWith("~/core/", StringComparison.OrdinalIgnoreCase)) {
                targetPath = corePoPath;
            }
            else if (current.StartsWith("~/themes/", StringComparison.OrdinalIgnoreCase)) {
                targetPath = GetThemeLocalizationPath(sitePath, current.Substring(9, current.IndexOf('/', 9) - 9));
            }
            else if (current.StartsWith("~/modules/", StringComparison.OrdinalIgnoreCase)) {
                targetPath = GetModuleLocalizationPath(sitePath, current.Substring(10, current.IndexOf('/', 10) - 10));
            }
            else {
                targetPath = rootPoPath;
            }
            WriteResourceString(GetBuilder(fileCatalog, targetPath), context, str);
        }

        private static readonly Regex FeatureNameExpression = new Regex(@"^\s+([^\s:]+):\s*$");

        private static void ExtractPoFromManifest(
            IDictionary<Path, StringBuilder> fileCatalog,
            Path poPath,
            string manifest,
            Path manifestPath,
            Path rootPath) {

            var context = "~/" + manifestPath.MakeRelativeTo(rootPath).ToString()
                                     .Replace('\\', '/');
            var reader = new StringReader(manifest);
            var builder = GetBuilder(fileCatalog, poPath);
            string line;
            while ((line = reader.ReadLine()) != null) {
                var split = line.Split(new[] {':'}, 2, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()).ToArray();
                if (split.Length == 2) {
                    var key = split[0];
                    if (new[] {"Name", "Description", "Author", "Website", "Tags"}.Contains(key)) {
                        var value = split[1];
                        WriteResourceString(
                            builder,
                            context,
                            '"' + key + '"',
                            '"' + value + '"');
                    }
                }
                if (line.StartsWith("Features:")) {
                    var feature = "";
                    while ((line = reader.ReadLine()) != null) {
                        var match = FeatureNameExpression.Match(line);
                        if (match.Success) {
                            feature = match.Groups[1].Value;
                            continue;
                        }
                        split = line.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim()).ToArray();
                        if (split.Length != 2) continue;
                        var key = split[0];
                        if (new[] { "Name", "Description", "Category" }.Contains(key)) {
                            var value = split[1];
                            WriteResourceString(
                                builder,
                                context,
                                '"' + feature + '.' + key + "\"",
                                '"' + value + '"');
                        }
                    }
                }
            }
        }

        private static Path GetThemeLocalizationPath(Path siteRoot, string themeName) {
            return siteRoot.Combine(
                "Themes", themeName, "App_Data",
                "Localization", "en-US",
                "orchard.theme.po");
        }

        private static Path GetModuleLocalizationPath(Path siteRoot, string moduleName) {
            return siteRoot.Combine(
                "Modules", moduleName, "App_Data",
                "Localization", "en-US",
                "orchard.module.po");
        }

        private static void WriteResourceString(StringBuilder builder, string context, string value) {
             WriteResourceString(builder, context, value, value);
        }

        private static void WriteResourceString(StringBuilder builder, string context, string key, string value) {
            builder.AppendLine("#: " + context);
            builder.AppendLine("#| msgid " + key);
            builder.AppendLine("msgid " + value);
            builder.AppendLine("msgstr " + value);
            builder.AppendLine();
        }
    }
}