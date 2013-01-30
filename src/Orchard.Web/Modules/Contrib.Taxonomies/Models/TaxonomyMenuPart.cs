using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace Contrib.Taxonomies.Models {
    /// <summary>
    /// Represents the part for the Taxonomy Menu Widget
    /// </summary>
    public class TaxonomyMenuPart : ContentPart<TaxonomyMenuPartRecord> {

        /// <summary>
        /// The taxonomy to display
        /// </summary>

        [Required]
        public TaxonomyPartRecord TaxonomyPartRecord {
            get { return Record.TaxonomyPartRecord; }
            set { Record.TaxonomyPartRecord = value; }
        }

        /// <summary>
        /// Top term to display in the menu.
        /// If null, the taxonomy is supposed to be the top term.
        /// </summary>
        public TermPartRecord TermPartRecord {
            get { return Record.TermPartRecord; }
            set { Record.TermPartRecord = value; }
        }

        /// <summary>
        /// Whether to display the root node or not.
        /// If <c>False</c>, the menu will have a flat first level.
        /// </summary>
        public bool DisplayTopMenuItem {
            get { return Record.DisplayTopMenuItem; }
            set { Record.DisplayTopMenuItem = value; }
        }

        /// <summary>
        /// How many sub-levels to display.
        /// If 0, then there will be only one level displayed (default)
        /// </summary>
        public int LevelsToDisplay {
            get { return Record.LevelsToDisplay; }
            set { Record.LevelsToDisplay = value; }
        }

        /// <summary>
        /// Whether to display the number of content items
        /// associated with this term, in the generated menu item text
        /// </summary>
        public bool DisplayContentCount {
            get { return Record.DisplayContentCount; }
            set { Record.DisplayContentCount = value; }
        }

        /// <summary>
        /// Whether to hide the terms without any associated content
        /// </summary>
        public bool HideEmptyTerms {
            get { return Record.HideEmptyTerms; }
            set { Record.HideEmptyTerms = value; }
        }
    }
}
