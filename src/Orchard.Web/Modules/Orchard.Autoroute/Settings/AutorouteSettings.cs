using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Services;
using System;

namespace Orchard.Autoroute.Settings {

    /// <summary>
    /// Settings when attaching part to a content item
    /// </summary>
    public class AutorouteSettings {

        private List<RoutePattern> _patterns;
        private List<DefaultPattern> _defaultPatterns;

        public AutorouteSettings() {
            PerItemConfiguration = false;
            AllowCustomPattern = true;
            UseCulturePattern = false;
            AutomaticAdjustmentOnEdit = false;
            PatternDefinitions = "[]";
            DefaultPatternDefinitions = "[]";
            DefaultPatternIndex = null;
        }

        public bool PerItemConfiguration { get; set; }
        public bool AllowCustomPattern { get; set; }
        public bool UseCulturePattern { get; set; }
        public bool AutomaticAdjustmentOnEdit { get; set; }
        public bool? IsDefault { get; set; }
        public List<string> SiteCultures { get; set; }
        public string DefaultSiteCulture { get; set; }
        public string DefaultPatternIndex { get; set; }

        /// <summary>
        /// A serialized Json array of <see cref="RoutePattern"/> objects
        /// </summary>
        public string PatternDefinitions { get; set; }

        public List<RoutePattern> Patterns {
            get {
                if (_patterns == null) {
                    _patterns = new DefaultJsonConverter().Deserialize<RoutePattern[]>(PatternDefinitions).ToList();
                }

                return _patterns;
            }

            set {
                _patterns = value;
                PatternDefinitions = new DefaultJsonConverter().Serialize(_patterns.ToArray());
            }
        }

        /// <summary>
        /// A serialized Json array of <see cref="DefaultPattern"/> objects
        /// </summary>
        public string DefaultPatternDefinitions { get; set; }

        public List<DefaultPattern> DefaultPatterns {
            get {
                if (_defaultPatterns == null) {
                    _defaultPatterns = new DefaultJsonConverter().Deserialize<DefaultPattern[]>(DefaultPatternDefinitions).ToList();
                }

                //We split the values from the radio button returned values
                int i = 0;
                foreach (DefaultPattern defaultPattern in _defaultPatterns) {
                    if (!String.IsNullOrWhiteSpace(defaultPattern.Culture)) {
                        if (defaultPattern.Culture.Split('|').Count() > 1) {
                            _defaultPatterns[i].PatternIndex = defaultPattern.Culture.Split('|').Last();
                            _defaultPatterns[i].Culture = defaultPattern.Culture.Split('|').First();
                        }
                    }
                    i++;
                }
                return _defaultPatterns;
            }

            set {
                _defaultPatterns = value;

                //We split the values from the radio button returned values
                int i = 0;
                foreach (DefaultPattern defaultPattern in _defaultPatterns) {
                    if (!String.IsNullOrWhiteSpace(defaultPattern.Culture)) {
                        if (defaultPattern.Culture.Split('|').Count() > 1) {
                            _defaultPatterns[i].PatternIndex = defaultPattern.Culture.Split('|').Last();
                            _defaultPatterns[i].Culture = defaultPattern.Culture.Split('|').First();
                        }
                    }
                    i++;
                }
                DefaultPatternDefinitions = new DefaultJsonConverter().Serialize(_defaultPatterns.ToArray());
            }
        }

        public void Build(ContentTypePartDefinitionBuilder builder) {
            builder.WithSetting("AutorouteSettings.PerItemConfiguration", PerItemConfiguration.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting("AutorouteSettings.AllowCustomPattern", AllowCustomPattern.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting("AutorouteSettings.UseCulturePattern", UseCulturePattern.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", AutomaticAdjustmentOnEdit.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting("AutorouteSettings.PatternDefinitions", PatternDefinitions);
            builder.WithSetting("AutorouteSettings.DefaultPatternDefinitions", DefaultPatternDefinitions);
            builder.WithSetting("AutorouteSettings.DefaultPatternIndex", DefaultPatternIndex);
        }
    }
}
