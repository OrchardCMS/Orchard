using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Autoroute.Models;
using Orchard.Autoroute.Settings;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Localization.Services;

namespace Orchard.Autoroute {
    public class Migrations : DataMigrationImpl {
        private readonly ICultureManager _cultureManager;

        public Migrations(ICultureManager cultureManager) {
            _cultureManager = cultureManager;
        }

        public int Create() {
            SchemaBuilder.CreateTable("AutoroutePartRecord", table => table
                .ContentPartVersionRecord()
                    .Column<string>("CustomPattern", c => c.WithLength(2048))
                    .Column<bool>("UseCustomPattern", c => c.WithDefault(false))
                    .Column<bool>("UseCulturePattern", c => c.WithDefault(false))
                    .Column<string>("DisplayAlias", c => c.WithLength(2048)));

            ContentDefinitionManager.AlterPartDefinition("AutoroutePart", part => part
                .Attachable()
                .WithDescription("Adds advanced url configuration options to your content type to completely customize the url pattern for a content item."));

            SchemaBuilder.AlterTable("AutoroutePartRecord", table => table
                .CreateIndex("IDX_AutoroutePartRecord_DisplayAlias", "DisplayAlias"));

            CreateCulturePatterns();

            return 5;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition("AutoroutePart", part => part
                .WithDescription("Adds advanced url configuration options to your content type to completely customize the url pattern for a content item."));

            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("AutoroutePartRecord", table => table
                .CreateIndex("IDX_AutoroutePartRecord_DisplayAlias", "DisplayAlias"));

            return 3;
        }

        public int UpdateFrom3() {
            SchemaBuilder.AlterTable("AutoroutePartRecord", table => table
                .AddColumn<bool>("UseCulturePattern", c => c.WithDefault(false)));

            return 4;
        }

        public int UpdateFrom4() {
            CreateCulturePatterns();

            return 5;
        }


        private void CreateCulturePatterns() {
            var autoroutePartDefinitions = ContentDefinitionManager.ListTypeDefinitions()
                .Where(type => type.Parts.Any(p => p.PartDefinition.Name == nameof(AutoroutePart)))
                .Select(type => new { ContentTypeName = type.Name, AutoroutePart = type.Parts.First(x => x.PartDefinition.Name == nameof(AutoroutePart)) });

            foreach (var partDefinition in autoroutePartDefinitions) {
                var settingsDictionary = partDefinition.AutoroutePart.Settings;
                var settings = settingsDictionary.GetModel<AutorouteSettings>();

                if (!settings.Patterns.Any(pattern => string.IsNullOrWhiteSpace(pattern.Culture))) {
                    var siteCulture = _cultureManager.GetSiteCulture();
                    List<string> newPatterns = new List<string>();

                    var siteCulturePatterns = settings.Patterns
                        .Where(pattern => string.Equals(pattern.Culture, siteCulture, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (siteCulturePatterns.Any()) {
                        foreach (RoutePattern pattern in siteCulturePatterns) {
                            newPatterns.Add($"{{\"Name\":\"{pattern.Name}\",\"Pattern\":\"{pattern.Pattern}\",\"Description\":\"{pattern.Description}\"}}");
                        }
                    }
                    else {
                        newPatterns.Add("{{\"Name\":\"Title\",\"Pattern\":\"{Content.Slug}\",\"Description\":\"my-title\"}}");
                    }

                    if (settingsDictionary.TryGetValue("AutorouteSettings.PatternDefinitions", out var oldPatterns) &&
                        oldPatterns.StartsWith("[") && oldPatterns.EndsWith("]")) {
                        newPatterns.Add(oldPatterns.Substring(1, oldPatterns.Length - 2));
                    }

                    ContentDefinitionManager.AlterTypeDefinition(partDefinition.ContentTypeName, type => type
                        .WithPart(nameof(AutoroutePart), builder => builder
                            .WithSetting("AutorouteSettings.PatternDefinitions", "[" + string.Join(",", newPatterns) + "]")));
                }
            }
        }
    }
}