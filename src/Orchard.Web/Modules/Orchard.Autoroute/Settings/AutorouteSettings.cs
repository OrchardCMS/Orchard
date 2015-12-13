using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Script.Serialization;
using Orchard.ContentManagement.MetaData.Builders;

namespace Orchard.Autoroute.Settings {

    /// <summary>
    /// Settings when attaching part to a content item
    /// </summary>
    public class AutorouteSettings {

        private List<RoutePattern> _patterns;

        public AutorouteSettings() {
            PerItemConfiguration = false;
            AllowCustomPattern = true;
            AutomaticAdjustmentOnEdit = false;
            PatternDefinitions = "[]";
        }

        public bool PerItemConfiguration { get; set; }
        public bool AllowCustomPattern { get; set; }
        public bool AutomaticAdjustmentOnEdit { get; set; }
        public int DefaultPatternIndex { get; set; }

        /// <summary>
        /// A serialized Json array of <see cref="RoutePattern"/> objects
        /// </summary>
        public string PatternDefinitions { get; set; }
        
        public List<RoutePattern> Patterns {
            get {
                if (_patterns == null) {
                    _patterns = new JavaScriptSerializer().Deserialize<RoutePattern[]>(PatternDefinitions).ToList();
                }

                return _patterns;
            }

            set { 
                _patterns = value;
                PatternDefinitions = new JavaScriptSerializer().Serialize(_patterns.ToArray());
            }
        }

        public void Build(ContentTypePartDefinitionBuilder builder) {
            builder.WithSetting("AutorouteSettings.PerItemConfiguration", PerItemConfiguration.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting("AutorouteSettings.AllowCustomPattern", AllowCustomPattern.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", AutomaticAdjustmentOnEdit.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting("AutorouteSettings.PatternDefinitions", PatternDefinitions);
            builder.WithSetting("AutorouteSettings.DefaultPatternIndex", DefaultPatternIndex.ToString(CultureInfo.InvariantCulture));
        }
    }
}
