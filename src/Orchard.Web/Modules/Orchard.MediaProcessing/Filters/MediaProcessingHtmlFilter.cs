using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Logging;
using Orchard.MediaProcessing.Models;
using Orchard.MediaProcessing.Services;
using Orchard.Services;

namespace Orchard.MediaProcessing.Filters {
    /// <summary>
    /// Resizes any images in HTML provided by parts that support IHtmlFilter and sets an alt text if not already supplied.
    /// </summary>
    [OrchardFeature(Features.OrchardMediaProcessingHtmlFilter)]
    public class MediaProcessingHtmlFilter : IHtmlFilter {

        private readonly IWorkContextAccessor _wca;
        private readonly IImageProfileManager _profileManager;

        private MediaHtmlFilterSettingsPart _settingsPart;
        private static readonly Regex _imageTagRegex = new Regex(@"<img\b[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly ConcurrentDictionary<string, Regex> _attributeRegexes = new ConcurrentDictionary<string, Regex>();
        private static readonly ConcurrentDictionary<string, string> _attributeValues = new ConcurrentDictionary<string, string>();
        private static readonly Dictionary<string, string> _validExtensions = new Dictionary<string, string> {
            { ".jpeg", "jpg" }, // For example: .jpeg supports compression (quality), format to 'jpg'.
            { ".jpg", "jpg"  },
            { ".png", null }
        };

        public MediaProcessingHtmlFilter(IWorkContextAccessor wca, IImageProfileManager profileManager) {
            _profileManager = profileManager;
            _wca = wca;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public MediaHtmlFilterSettingsPart Settings {
            get {
                if (_settingsPart == null) {
                    _settingsPart = _wca.GetContext().CurrentSite.As<MediaHtmlFilterSettingsPart>();
                }

                return _settingsPart;
            }
        }

        public string ProcessContent(string text, HtmlFilterContext context) {
            if (string.IsNullOrWhiteSpace(text) || context.Flavor != "html") {
                return text;
            }

            var matches = _imageTagRegex.Matches(text);

            if (matches.Count == 0) {
                return text;
            }

            var offset = 0; // This tracks where last image tag ended in the original HTML.
            var newText = new StringBuilder();

            foreach (Match match in matches) {
                newText.Append(text.Substring(offset, match.Index - offset));
                offset = match.Index + match.Length;
                var imgTag = match.Value;
                var processedImgTag = ProcessImageContent(imgTag);

                if (Settings.PopulateAlt) {
                    processedImgTag = ProcessImageAltContent(processedImgTag);
                }

                newText.Append(processedImgTag);
            }

            newText.Append(text.Substring(offset));

            return newText.ToString();
        }

        private string ProcessImageContent(string imgTag) {
            if (imgTag.Contains("noresize")) {
                return imgTag;
            }

            var src = GetAttributeValue(imgTag, "src");
            var ext = string.IsNullOrEmpty(src) ? null : Path.GetExtension(src).ToLowerInvariant();
            var width = GetAttributeValueInt(imgTag, "width");
            var height = GetAttributeValueInt(imgTag, "height");

            if (width > 0 && height > 0
                && !string.IsNullOrEmpty(src)
                && !src.Contains("_Profiles")
                && _validExtensions.ContainsKey(ext)) {
                try {
                    // If the image has a combination of width, height and valid extension, that is not already in
                    // _Profiles, then process the image.
                    var newSrc = TryGetImageProfilePath(src, ext, width, height);
                    imgTag = SetAttributeValue(imgTag, "src", newSrc);
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Unable to process Html Dynamic image profile for '{0}'", src);
                }
            }

            return imgTag;
        }

        private string TryGetImageProfilePath(string src, string ext, int width, int height) {
            var filters = new List<FilterRecord> {
                // Factor in a minimum width and height with respect to higher pixel density devices.
                CreateResizeFilter(width * Settings.DensityThreshold, height * Settings.DensityThreshold)
            };

            if (_validExtensions[ext] != null && Settings.Quality < 100) {
                filters.Add(CreateFormatFilter(Settings.Quality, _validExtensions[ext]));
            }

            var profileName = string.Format(
                "Transform_Resize_w_{0}_h_{1}_m_Stretch_a_MiddleCenter_c_{2}_d_@{3}x",
                width,
                height,
                Settings.Quality,
                Settings.DensityThreshold);

            return _profileManager.GetImageProfileUrl(src, profileName, null, filters.ToArray());
        }

        private FilterRecord CreateResizeFilter(int width, int height) {
            // Because the images can be resized in the HTML editor, we must assume that the image is of the exact desired
            // dimensions and that stretch is an appropriate mode. Note that the default is to never upscale images.
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

        private string ProcessImageAltContent(string imgTag) {
            var src = GetAttributeValue(imgTag, "src");
            var alt = GetAttributeValue(imgTag, "alt");

            if (string.IsNullOrEmpty(alt) && !string.IsNullOrEmpty(src)) {
                var text = Path.GetFileNameWithoutExtension(src).Replace("-", " ").Replace("_", " ");
                imgTag = SetAttributeValue(imgTag, "alt", text);
            }

            return imgTag;
        }

        private string GetAttributeValue(string tag, string attributeName) =>
            _attributeValues
                .GetOrAdd($"{tag}_{attributeName}", _ => {
                    var match = GetAttributeRegex(attributeName).Match(tag);
                    return match.Success ? match.Groups[1].Value : null;
                });

        private int GetAttributeValueInt(string tag, string attributeName) =>
            int.TryParse(GetAttributeValue(tag, attributeName), out int result) ? result : 0;

        private string SetAttributeValue(string tag, string attributeName, string value) {
            var attributeRegex = GetAttributeRegex(attributeName);
            var newAttribute = $"{attributeName}=\"{value}\"";

            if (attributeRegex.IsMatch(tag)) {
                return attributeRegex.Replace(tag, newAttribute);
            }
            else {
                return tag.Insert(tag.Length - 1, $" {newAttribute}");
            }
        }

        private Regex GetAttributeRegex(string attributeName) =>
            _attributeRegexes.GetOrAdd(
                attributeName,
                name => new Regex($@"\b{name}\s*=\s*[""']?([^""'\s>]*)[""']?", RegexOptions.IgnoreCase | RegexOptions.Compiled));
    }
}
