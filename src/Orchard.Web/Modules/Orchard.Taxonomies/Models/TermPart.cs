using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Title.Models;

namespace Orchard.Taxonomies.Models
{
    public class TermPart : ContentPart<TermPartRecord>
    {
        public string Name
        {
            get { return this.As<TitlePart>().Title; }
            set { this.As<TitlePart>().Title = value; }
        }

        public string Slug
        {
            get { return this.As<AutoroutePart>().DisplayAlias; }
            set { this.As<AutoroutePart>().DisplayAlias = value; }
        }

        public IContent Container
        {
            get { return this.As<ICommonPart>().Container; }
            set { this.As<ICommonPart>().Container = value; }
        }

        public int TaxonomyId
        {
            get { return Retrieve(x => x.TaxonomyId); }
            set { Store(x => x.TaxonomyId, value); }
        }

        /// <summary>
        /// e.g., /; /1/; /1/2/
        /// </summary>
        public string Path
        {
            get { return Retrieve(x => x.Path); }
            set { Store(x => x.Path, value); }
        }

        public int Count
        {
            get { return Retrieve(x => x.Count); }
            set { Store(x => x.Count, value); }
        }

        public bool Selectable
        {
            get { return Retrieve(x => x.Selectable); }
            set { Store(x => x.Selectable, value); }
        }

        /// <summary>
        /// Property used to sort terms that have the same level or path
        /// </summary>
        [Range(-524287, 524288, ErrorMessage = "Valid Weight is between -524287 and 524288")]
        public int Weight
        {
            get { return Retrieve(x => x.Weight); }
            set { Store(x => x.Weight, value); }
        }

        /// <summary>
        /// This property is used to represent the lexicographic order of the term inside the taxonomy.
        /// The term FullWeight is composed by his parent FullWeight and the lexicographic representation of the own term separated with a slash '/'.
        /// See TaxonomyService.ComputeFullWeight for the details of the implementation.
        /// </summary>
        public string FullWeight
        {
            get { return Record.FullWeight; }
            set { Record.FullWeight = value; }
        }

        public string FullPath => string.Concat(Path, Id);

        public static IEnumerable<TermPart> Sort(IEnumerable<TermPart> terms)
        {
            return terms.OrderBy(term => term.FullWeight);
        }
    }
}
