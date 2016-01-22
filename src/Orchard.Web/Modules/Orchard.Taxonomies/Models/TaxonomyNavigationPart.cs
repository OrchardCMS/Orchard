using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;

namespace Orchard.Taxonomies.Models {
    public class TaxonomyNavigationPart : ContentPart {

        /// <summary>
        /// The taxonomy to display
        /// </summary>
        public int TaxonomyId {
            get { return Convert.ToInt32(this.As<InfosetPart>().Get("TaxonomyNavigationPart", "TaxonomyId", null), CultureInfo.InvariantCulture); }
            set { this.As<InfosetPart>().Set("TaxonomyNavigationPart", "TaxonomyId", null, Convert.ToString(value, CultureInfo.InvariantCulture)); }
        }

        /// <summary>
        /// Top term to display in the menu.
        /// If null, the taxonomy is supposed to be the top term.
        /// </summary>
        public int TermId {
            get { return Convert.ToInt32(this.As<InfosetPart>().Get("TaxonomyNavigationPart", "TermId", null), CultureInfo.InvariantCulture); }
            set { this.As<InfosetPart>().Set("TaxonomyNavigationPart", "TermId", null, Convert.ToString(value, CultureInfo.InvariantCulture)); }
        }

        /// <summary>
        /// Whether to display the root node or not.
        /// If <c>False</c>, the menu will have a flat first level.
        /// </summary>
        public bool DisplayRootTerm {
            get { return Convert.ToBoolean(this.As<InfosetPart>().Get("TaxonomyNavigationPart", "DisplayRootTerm", null), CultureInfo.InvariantCulture); }
            set { this.As<InfosetPart>().Set("TaxonomyNavigationPart", "DisplayRootTerm", null, Convert.ToString(value, CultureInfo.InvariantCulture)); }
        }

        /// <summary>
        /// Number of levels to render. If <value>0</value> all levels are rendered.
        /// </summary>
        public int LevelsToDisplay {
            get { return Convert.ToInt32(this.As<InfosetPart>().Get("TaxonomyNavigationPart", "LevelsToDisplay"), CultureInfo.InvariantCulture); }
            set { this.As<InfosetPart>().Set("TaxonomyNavigationPart", "LevelsToDisplay", Convert.ToString(value, CultureInfo.InvariantCulture)); }
        }

        /// <summary>
        /// Whether to display the number of content items
        /// associated with this term, in the generated menu item text
        /// </summary>
        public bool DisplayContentCount {
            get { return Convert.ToBoolean(this.As<InfosetPart>().Get("TaxonomyNavigationPart", "DisplayContentCount", null), CultureInfo.InvariantCulture); }
            set { this.As<InfosetPart>().Set("TaxonomyNavigationPart", "DisplayContentCount", null, Convert.ToString(value, CultureInfo.InvariantCulture)); }
        }

        /// <summary>
        /// Whether to hide the terms without any associated content
        /// </summary>
        public bool HideEmptyTerms {
            get { return Convert.ToBoolean(this.As<InfosetPart>().Get("TaxonomyNavigationPart", "HideEmptyTerms", null), CultureInfo.InvariantCulture); }
            set { this.As<InfosetPart>().Set("TaxonomyNavigationPart", "HideEmptyTerms", null, Convert.ToString(value, CultureInfo.InvariantCulture)); }
        }
    }
}