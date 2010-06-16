using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.Core.Contents.Services {
    public class ContentDefinitionService : IContentDefinitionService {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentDefinitionService(IOrchardServices services, IContentDefinitionManager contentDefinitionManager) {
            Services = services;
            _contentDefinitionManager = contentDefinitionManager;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public IEnumerable<ContentTypeDefinition> GetTypeDefinitions() {
            return _contentDefinitionManager.ListTypeDefinitions();
        }

        public ContentTypeDefinition GetTypeDefinition(string name) {
            throw new NotImplementedException();
        }

        public void AddTypeDefinition(ContentTypeDefinitionStub definitionStub) {
            if (string.IsNullOrWhiteSpace(definitionStub.Name))
                definitionStub.Name = GenerateTypeName(definitionStub.DisplayName);

            while (_contentDefinitionManager.GetTypeDefinition(definitionStub.Name) != null)
                definitionStub.Name = VersionTypeName(definitionStub.Name);

            //just giving the new type some default parts for now
            _contentDefinitionManager.AlterTypeDefinition(
                definitionStub.Name,
                cfg => cfg.Named(definitionStub.Name, definitionStub.DisplayName)
                           .WithPart("CommonAspect")
                           //.WithPart("RoutableAspect") //need to go the new routable route
                           .WithPart("BodyAspect"));

            Services.Notifier.Information(T("Created content type: {0}", definitionStub.DisplayName));
        }

        public void RemoveTypeDefinition(string name) {
            throw new NotImplementedException();
        }

        //gratuitously stolen from the RoutableService
        private static string GenerateTypeName(string displayName) {
            if (string.IsNullOrWhiteSpace(displayName))
                return "";

            var name = displayName;
            //todo: might need to be made more restrictive depending on how name is used (like as an XML node name, for instance)
            var dissallowed = new Regex(@"[/:?#\[\]@!$&'()*+,;=\s]+");

            name = dissallowed.Replace(name, "-");
            name = name.Trim('-');

            if (name.Length > 128)
                name = name.Substring(0, 128);

            return name.ToLowerInvariant();
        }

        private static string VersionTypeName(string name) {
            var version = 2;
            var nameParts = name.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

            if (nameParts.Length > 1 && int.TryParse(nameParts.Last(), out version)) {
                version = version > 0 ? ++version : 2;
                //this could unintentionally chomp something that looks like a version
                name = string.Join("-", nameParts.Take(nameParts.Length - 1));
            }

            return string.Format("{0}-{1}", name, version);
        }
    }
}