using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.Caching;
using Orchard.FileSystems.WebSite;
using Orchard.Logging;

namespace Orchard.DisplayManagement.Descriptors.ShapePlacementStrategy {

    /// <summary>
    /// Parses and caches the Placement.info file contents for a given IWebSiteFolder vdir
    /// </summary>
    public interface IPlacementFileParser : IDependency {
        PlacementFile Parse(string virtualPath);
        PlacementFile ParseText(string placementText);
    }


    public class PlacementFileParser : IPlacementFileParser {
        private readonly ICacheManager _cacheManager;
        private readonly IWebSiteFolder _webSiteFolder;

        public PlacementFileParser(ICacheManager cacheManager, IWebSiteFolder webSiteFolder) {
            _cacheManager = cacheManager;
            _webSiteFolder = webSiteFolder;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public bool DisableMonitoring { get; set; }

        public PlacementFile Parse(string virtualPath) {
            return _cacheManager.Get(virtualPath, true, context => {

                if (!DisableMonitoring) {
                    Logger.Debug("Monitoring virtual path \"{0}\"", virtualPath);
                    context.Monitor(_webSiteFolder.WhenPathChanges(virtualPath));
                }

                var placementText = _webSiteFolder.ReadFile(virtualPath);
                return ParseText(placementText);
            });
        }

        public PlacementFile ParseText(string placementText) {
            if (placementText == null)
                return null;


            var element = XElement.Parse(placementText);
            return new PlacementFile {
                Nodes = Accept(element).ToList()
            };
        }

        private IEnumerable<PlacementNode> Accept(XElement element) {
            switch (element.Name.LocalName) {
                case "Placement":
                    return AcceptMatch(element);
                case "Match":
                    return AcceptMatch(element);
                case "Place":
                    return AcceptPlace(element);
            }
            return Enumerable.Empty<PlacementNode>();
        }


        private IEnumerable<PlacementNode> AcceptMatch(XElement element) {
            if (element.HasAttributes == false) {
                // Match with no attributes will collapse child results upward
                // rather than return an unconditional node
                return element.Elements().SelectMany(Accept);
            }

            // return match node that carries back key/value dictionary of condition,
            // and has child rules nested as Nodes
            return new[]{new PlacementMatch{
                Terms = element.Attributes().ToDictionary(attr=>attr.Name.LocalName, attr=>attr.Value),
                Nodes=element.Elements().SelectMany(Accept).ToArray(),
            }};
        }

        private IEnumerable<PlacementShapeLocation> AcceptPlace(XElement element) {
            // return attributes as part locations
            return element.Attributes().Select(attr => new PlacementShapeLocation {
                ShapeType = attr.Name.LocalName,
                Location = attr.Value
            });
        }

    }
}

