using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Taxonomies.Models;

namespace Orchard.Taxonomies.Services {
    public interface ITaxonomyService : IDependency {
        /// <summary>
        /// Returns all the <see cref="TaxonomyPart" /> content items.
        /// </summary>
        /// <returns>The <see cref="TaxonomyPart"/> content items.</returns>
        IEnumerable<TaxonomyPart> GetTaxonomies();

        /// <summary>
        /// Loads the published version of <see cref="TaxonomyPart"/> content item by its id.
        /// </summary>
        /// <param name="id">The id of the <see cref="TaxonomyPart"/> to load.</param>
        /// <returns>The <see cref="TaxonomyPart"/> with the specified id or <value>null</value> if no published version of this id exists.</returns>
        TaxonomyPart GetTaxonomy(int id);

        /// <summary>
        /// Loads the published version of <see cref="TaxonomyPart"/> content item by its name.
        /// </summary>
        /// <param name="name">The name of the <see cref="TaxonomyPart"/> to load.</param>
        /// <returns>The <see cref="TaxonomyPart"/> with the specified id or <value>null</value> if no published version of this name exists.</returns>
        TaxonomyPart GetTaxonomyByName(string name);

        /// <summary>
        /// Creates a new Content Type for the terms of a <see cref="TaxonomyPart"/>
        /// </summary>
        /// <param name="taxonomy">The taxonomy to create a term content type for.</param>
        void CreateTermContentType(TaxonomyPart taxonomy);
        
        /// <summary>
        /// Deletes a <see cref="TaxonomyPart"/> content item from the database.
        /// </summary>
        /// <param name="taxonomy">The taxonomy to delete.</param>
        /// <remarks>It will also remove all its terms and delete their content type.</remarks>
        void DeleteTaxonomy(TaxonomyPart taxonomy);

        IEnumerable<TermPart> GetTerms(int taxonomyId);
        TermPart GetTerm(int id);
        TermPart GetTermByName(int taxonomyId, string name);
        void DeleteTerm(TermPart termPart);
        void MoveTerm(TaxonomyPart taxonomy, TermPart term, TermPart parentTerm);
        void ProcessPath(TermPart term);

        string GenerateTermTypeName(string taxonomyName);

        /// <summary>
        /// Creates a new <see cref="TermPart"/> content item in memory.
        /// </summary>
        /// <remarks>You still need to assign the Name propery and create the content item in the database.</remarks>
        /// <param name="taxonomy">The <see cref="TaxonomyPart"/> content item the new term is associated to.</param>
        /// <returns>A new instance of <see cref="TermPart"/></returns>
        TermPart NewTerm(TaxonomyPart taxonomy);

        /// <summary>
        /// Creates a new <see cref="TermPart"/> content item in memory.
        /// </summary>
        /// <remarks>You still need to assign the Name propery and create the content item in the database.</remarks>
        /// <param name="taxonomy">The <see cref="TaxonomyPart"/> content item the new term is associated to.</param>
        /// <param name="parent">The <see cref="IContent"/> instance this term is a child of. This can be a <see cref="TermPart"/> of the same taxonomy or the taxonomy itself.</param>
        /// <returns>A new instance of <see cref="TermPart"/></returns>
        TermPart NewTerm(TaxonomyPart taxonomy, IContent parent);

        IEnumerable<TermPart> GetTermsForContentItem(int contentItemId, string field = null, VersionOptions versionOptions = null);
        void UpdateTerms(ContentItem contentItem, IEnumerable<TermPart> terms, string field);
        IEnumerable<TermPart> GetParents(TermPart term);
        IEnumerable<TermPart> GetChildren(TermPart term);
        IEnumerable<TermPart> GetChildren(TermPart term, bool includeParent);
        IEnumerable<IContent> GetContentItems(TermPart term, int skip = 0, int count = 0, string fieldName = null);
        long GetContentItemsCount(TermPart term, string fieldName = null);
        IContentQuery<TermsPart, TermsPartRecord> GetContentItemsQuery(TermPart term, string fieldName = null);

        /// <summary>
        /// Organizes a list of <see cref="TermPart"/> objects into a hierarchy.
        /// </summary>
        /// <param name="terms">The <see cref="TermPart"/> objects to orgnanize in a hierarchy. The objects need to be sorted.</param>
        /// <param name="append">The action to perform when a node is added as a child, or <c>null</c> if nothing needs to be done.</param>
        void CreateHierarchy(IEnumerable<TermPart> terms, Action<TermPartNode, TermPartNode> append);
    }
}