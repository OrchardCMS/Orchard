using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Utility;

namespace Orchard.CmsPages.Services.Templates {
    public interface ITemplateProvider : IDependency {
        IList<TemplateDescriptor> List();
        TemplateDescriptor Get(string name);
    }

    public class TemplateProvider : ITemplateProvider {
        private readonly ITemplateEntryProvider _entryProvider;

        public TemplateProvider(ITemplateEntryProvider entryProvider) {
            _entryProvider = entryProvider;
        }

        public IList<TemplateDescriptor> List() {
            var result = new List<TemplateDescriptor>();
            foreach (var entry in _entryProvider.List()) {
                TemplateDescriptor descriptor = CreateDescriptor(entry);

                result.Add(descriptor);
            }
            return result.ToReadOnlyCollection();
        }

        public TemplateDescriptor Get(string name) {
            return _entryProvider.List()
                .Where(entry => entry.Name == name)
                .Select(entry => CreateDescriptor(entry))
                .SingleOrDefault();
        }

        private static TemplateDescriptor CreateDescriptor(TemplateEntry entry) {
            var parser = new MetadataParser();
            var metadataEntries = parser.Parse(new CommentExtractor().FirstComment(entry.Content));

            var descriptor = new TemplateDescriptor {Name = entry.Name};

            foreach (var metadataEntry in metadataEntries) {
                switch (metadataEntry.Tag.ToLower()) {
                    case "name":
                        descriptor.DisplayName = metadataEntry.Value;
                        break;

                    case "description":
                        descriptor.Description = metadataEntry.Value;
                        break;

                    case "author":
                        descriptor.Author = metadataEntry.Value;
                        break;

                    case "zones":
                        string[] zones = metadataEntry.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var zone in zones) {
                            descriptor.Zones.Add(zone.Trim());
                        }
                        break;

                    default:
                        descriptor.Others.Add(metadataEntry);
                        break;
                }
            }

            return descriptor;
        }
    }
}