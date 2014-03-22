using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using HtmlAgilityPack;
using Orchard.Azure.MediaServices.Models;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Services;

namespace Orchard.Azure.MediaServices.Services.Rendering {
    public class CloudVideoFilter : IHtmlFilter {
        private readonly IShapeFactory _shapeFactory;
        private readonly IContentManager _contentManager;
        private readonly IShapeDisplay _shapeDisplay;

        public CloudVideoFilter(IShapeFactory shapeFactory, IContentManager contentManager, IShapeDisplay shapeDisplay) {
            _shapeFactory = shapeFactory;
            _contentManager = contentManager;
            _shapeDisplay = shapeDisplay;
        }

        public string ProcessContent(string text, string flavor) {
            return String.Equals(flavor, "html", StringComparison.OrdinalIgnoreCase) ? ReplaceVideoPlaceholder(text) : text;
        }

        private string ReplaceVideoPlaceholder(string text) {
            var document = ParseHtml(text);
            var cloudVideoNodes = HarvestCloudVideoNodes(document);
            var cloudVideos = cloudVideoNodes.Any() ? _contentManager.GetMany<CloudVideoPart>(cloudVideoNodes.Keys, VersionOptions.Published, QueryHints.Empty).ToDictionary(x => x.Id) : default(IDictionary<int, CloudVideoPart>);

            if (cloudVideos == null || !cloudVideos.Any())
                return text;

            foreach (var item in cloudVideoNodes) {
                var videoId = item.Key;
                var cloudVideoPart = cloudVideos.ContainsKey(videoId) ? cloudVideos[videoId] : default(CloudVideoPart);

                if (cloudVideoPart == null)
                    continue;

                var node = item.Value;
                var playerShape = CreatePlayershape(cloudVideoPart, node);
                var playerHtml = _shapeDisplay.Display(playerShape);

                node.Attributes.RemoveAll();
                node.InnerHtml = playerHtml;
            }

            return ToHtml(document);
        }

        private static HtmlDocument ParseHtml(string html) {
            var document = new HtmlDocument();
            document.LoadHtml(html);
            return document;
        }

        private static string ToHtml(HtmlDocument document) {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb)) {
                document.Save(writer);
                return sb.ToString();
            }
        }

        private IShape CreatePlayershape(CloudVideoPart part, HtmlNode node) {
            var playerWidth = GetInt32(node, "data-player-width");
            var playerHeight = GetInt32(node, "data-player-height");
            var applyMediaQueries = GetBoolean(node, "data-player-apply-media-queries");
            var autoPlay = GetBoolean(node, "data-player-auto-play");

            var playerShape = _shapeFactory.Create("CloudVideoPlayer", Arguments.From(new {
                CloudVideoPart = part,
                AssetId = default(int?),
                IgnoreIncludeInPlayer = false,
                AllowPrivateUrls = false,
                PlayerWidth = playerWidth,
                PlayerHeight = playerHeight,
                ApplyMediaQueries = applyMediaQueries,
                AutoPlay = autoPlay
            }));

            return playerShape;
        }

        private static IDictionary<int, HtmlNode> HarvestCloudVideoNodes(HtmlDocument bodyNode) {
            var dictionary = new Dictionary<int, HtmlNode>();
            var cloudVideoNodes = bodyNode.DocumentNode.SelectNodes("//div[@data-player-video-id]");

            if (cloudVideoNodes == null)
                return dictionary;

            foreach (var node in cloudVideoNodes) {
                var videoId = GetInt32(node, "data-player-video-id");
                
                if (videoId > 0) {
                    dictionary[videoId] = node;
                }
            }

            return dictionary;
        }

        private static int GetInt32(HtmlNode node, string attributeName, int defaultValue = default(int)) {
            var attribute = node.Attributes[attributeName];
            return attribute != null ? XmlConvert.ToInt32(attribute.Value) : defaultValue;
        }

        private static bool GetBoolean(HtmlNode node, string attributeName, bool defaultValue = default(bool)) {
            var attribute = node.Attributes[attributeName];
            return attribute != null ? XmlConvert.ToBoolean(attribute.Value) : defaultValue;
        }
    }
}