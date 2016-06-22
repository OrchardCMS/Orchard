using HtmlAgilityPack;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.Media;
using Orchard.Forms.Services;
using Orchard.Logging;
using Orchard.MediaProcessing.Models;
using Orchard.MediaProcessing.Services;
using Orchard.Services;
using Orchard.Settings;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace Orchard.MediaProcessing.Filters {
    /// <summary>
    /// Resizes any images in HTML provided by parts that support IHtmlFilter and sets an alt text if not already supplied
    /// </summary>
    [OrchardFeature(Features.OrchardMediaProcessingHtmlFilter)]
    public class MediaProcessingHtmlFilter : IHtmlFilter {

        private readonly IWorkContextAccessor _wca;
        private readonly IImageProcessingFileNameProvider _fileNameProvider;
        private readonly IImageProfileManager _profileManager;
        private readonly IStorageProvider _storageProvider;

        private MediaHtmlFilterSettingsPart _settingsPart;
        private Dictionary<string, string> _validExtensions = new Dictionary<string, string> {
            { ".jpeg", "jpg" }, //For example: .jpeg supports commpression (quality), format to 'jpg'
            { ".jpg", "jpg"  },
            { ".png", null }};

        public MediaProcessingHtmlFilter(IWorkContextAccessor wca, IImageProcessingFileNameProvider fileNameProvider, IImageProfileManager profileManager, IStorageProvider storageProvider) {
            _fileNameProvider = fileNameProvider;
            _profileManager = profileManager;
            _storageProvider = storageProvider;
            _wca = wca;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public MediaHtmlFilterSettingsPart Settings { get {
                if (_settingsPart == null) {
                    _settingsPart = _wca.GetContext().CurrentSite.As<MediaHtmlFilterSettingsPart>();
                }
                return _settingsPart;
            }
        }

        public string ProcessContent(string text, string flavor) {
            if (!string.IsNullOrEmpty(text) && flavor == "html") {
                var doc = new HtmlDocument();
                doc.LoadHtml(text);
                foreach (var node in doc.DocumentNode.DescendantsAndSelf("img")) {
                    ProcessImageContent(node);
                    if (Settings.PopulateAlt) {
                        ProcessImageAltContent(node);
                    }
                }
                return doc.DocumentNode.OuterHtml;
            }
            else {
                return text;
            }
        }

        private void ProcessImageContent(HtmlNode node) {
            // If the noresize attribute is present, do nothing.
            if (node.Attributes.AttributesWithName("noresize").Any()) {
                return;
            }

            var src = GetAttributeValue(node, "src");
            var ext = string.IsNullOrEmpty(src) ? null : Path.GetExtension(src);
            var width = GetAttributeValueInt(node, "width");
            var height = GetAttributeValueInt(node, "height");

            if (width > 0 && height > 0 && !string.IsNullOrEmpty(src) && !src.Contains("_Profiles") && _validExtensions.ContainsKey(ext)) {
                try {
                    //If img has a width, height, not already in _Profiles and a valid ext, process the image.
                    node.Attributes["src"].Value = TryGetImageProfilePath(src, ext, width, height);
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Unable to process Html Dynamic image profile for '{0}'", src);
                }
            }
        }

        private string TryGetImageProfilePath(string src, string ext, int width, int height) {
            
            var filters = new List<FilterRecord>();
            filters.Add(CreateResizeFilter(width * Settings.DensityThreshold, height * Settings.DensityThreshold)); // Factor in a min height and width with respect to higher pixel density devices.

            // If the ext supports compression, also set the quality.
            if (_validExtensions[ext] != null && Settings.Quality < 100) {
                filters.Add(CreateFormatFilter(Settings.Quality, _validExtensions[ext]));
            }

            var profileName = string.Format("Transform_Resize_w_{0}_h_{1}_m_Stretch_a_MiddleCenter_c_{2}_d_@{3}x", width, height, Settings.Quality, Settings.DensityThreshold);
            return _profileManager.GetImageProfileUrl(src, profileName, null, filters.ToArray());

        }

        private FilterRecord CreateResizeFilter(int width, int height) {
            // Because the images can be resized in the html editor, we must assume that the image
            // is of the exact desired dimensions and that stretch is an appropriate mode.
            // Note that the default is to never upscale images.
            var state = new Dictionary<string, string> {
                { "Width", width.ToString() },
                { "Height", height.ToString() },
                { "Mode", "Stretch" },
                { "Alignment", "MiddleCenter" },
                { "PadColor", "" }
            };

            return new FilterRecord {
                Category = "Transform",
                Type = "Resize",
                State = FormParametersHelper.ToString(state)
            };
        }

        private FilterRecord CreateFormatFilter(int quality, string format) {
            var state = new Dictionary<string, string> {
                { "Quality", quality.ToString() },
                { "Format", format },
            };

            return new FilterRecord {
                Category = "Transform",
                Type = "Format",
                State = FormParametersHelper.ToString(state)
            };
        }

        private void ProcessImageAltContent(HtmlNode node) {
            var src = GetAttributeValue(node, "src");
            var alt = GetAttributeValue(node, "alt");
            if (string.IsNullOrEmpty(alt) && !string.IsNullOrEmpty(src)) {
                var text = Path.GetFileNameWithoutExtension(src).Replace("-", " ").Replace("_", " ");
                SetAttributeValue(node, "alt", text);
            }
        }

        private string GetAttributeValue(HtmlNode node, string name) {
            return node.Attributes[name] == null ? null : node.Attributes[name].Value;
        }

        private int GetAttributeValueInt(HtmlNode node, string name) {
            int val = 0;
            return node.Attributes[name] == null ? 0 : int.TryParse(node.Attributes[name].Value, out val) ? val : 0;
        }

        private void SetAttributeValue(HtmlNode node, string name, string value) {
            if (node.Attributes.Contains(name)) {
                node.Attributes[name].Value = value;
            }
            else {
                node.Attributes.Add(name, value);
            }
        }

    }
}