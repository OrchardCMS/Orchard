using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.ContentTypes.Services {
    public class ContentDefinitionService : IContentDefinitionService {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentFieldDriver> _contentFieldDrivers;

        public ContentDefinitionService(IOrchardServices services, IContentDefinitionManager contentDefinitionManager, IEnumerable<IContentFieldDriver> contentFieldDrivers) {
            Services = services;
            _contentDefinitionManager = contentDefinitionManager;
            _contentFieldDrivers = contentFieldDrivers;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public IEnumerable<ContentTypeDefinition> GetTypeDefinitions() {
            return _contentDefinitionManager.ListTypeDefinitions();
        }

        public ContentTypeDefinition GetTypeDefinition(string name) {
            return _contentDefinitionManager.GetTypeDefinition(name);
        }

        public void AddTypeDefinition(ContentTypeDefinition contentTypeDefinition) {
            var typeName = string.IsNullOrWhiteSpace(contentTypeDefinition.Name)
                ? GenerateTypeName(contentTypeDefinition.DisplayName)
                : contentTypeDefinition.Name;

            while (_contentDefinitionManager.GetTypeDefinition(typeName) != null)
                typeName = VersionTypeName(typeName);

            //just giving the new type some default parts for now
            _contentDefinitionManager.AlterTypeDefinition(
                typeName,
                cfg => cfg.DisplayedAs(contentTypeDefinition.DisplayName)
                           .WithPart("CommonAspect")
                           //.WithPart("RoutableAspect") //need to go the new routable route
                           .WithPart("BodyAspect"));
             
            Services.Notifier.Information(T("Created content type: {0}", contentTypeDefinition.DisplayName));
        }

        public void AlterTypeDefinition(ContentTypeDefinition contentTypeDefinition) {
            _contentDefinitionManager.StoreTypeDefinition(contentTypeDefinition);

            var implicitTypePart = contentTypeDefinition.Parts.SingleOrDefault(p => p.PartDefinition.Name == contentTypeDefinition.Name);
            if (implicitTypePart != null) {
                AlterPartDefinition(implicitTypePart.PartDefinition);
            }
        }

        public void RemoveTypeDefinition(string name) {
            throw new NotImplementedException();
        }

        public ContentPartDefinition GetPartDefinition(string name) {
            return _contentDefinitionManager.GetPartDefinition(name);
        }

        public void AddPartDefinition(ContentPartDefinition contentPartDefinition) {
            throw new NotImplementedException();
        }

        public void AlterPartDefinition(ContentPartDefinition contentPartDefinition) {
            _contentDefinitionManager.StorePartDefinition(contentPartDefinition);
        }

        public void RemovePartDefinition(string name) {
            throw new NotImplementedException();
        }

        public IEnumerable<ContentFieldInfo> GetFieldDefinitions() {
            return _contentFieldDrivers.SelectMany(d => d.GetFieldInfo());
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
            int version;
            var nameParts = name.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

            if (nameParts.Length > 1 && int.TryParse(nameParts.Last(), out version)) {
                version = version > 0 ? ++version : 2;
                //this could unintentionally chomp something that looks like a version
                name = string.Join("-", nameParts.Take(nameParts.Length - 1));
            }
            else {
                version = 2;
            }

            return string.Format("{0}-{1}", name, version);
        }
    }
}