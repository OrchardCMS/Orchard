using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Taxonomies.Models;

namespace Orchard.Taxonomies.Services {
    public interface ITaxonomyService : IDependency {
        IEnumerable<TaxonomyPart> GetTaxonomies();
        TaxonomyPart GetTaxonomy(int id);
        TaxonomyPart GetTaxonomyByName(string name);
        TaxonomyPart GetTaxonomyBySlug(string slug);
        void CreateTermContentType(TaxonomyPart taxonomy);
        void DeleteTaxonomy(TaxonomyPart taxonomy);

        IEnumerable<TermPart> GetAllTerms();
        IEnumerable<TermPart> GetTerms(int taxonomyId);
        TermPart GetTerm(int id);
        TermPart GetTermByPath(string path);
        TermPart GetTermByName(int taxonomyId, string name);
        void DeleteTerm(TermPart termPart);
        void MoveTerm(TaxonomyPart taxonomy, TermPart term, TermPart parentTerm);
        void ProcessPath(TermPart term);
        IEnumerable<string> GetTermPaths();

        string GenerateTermTypeName(string taxonomyName);
        TermPart NewTerm(TaxonomyPart taxonomy);
        IEnumerable<TermPart> GetTermsForContentItem(int contentItemId, string field = null, VersionOptions versionOptions = null);
        void UpdateTerms(ContentItem contentItem, IEnumerable<TermPart> terms, string field);
        IEnumerable<TermPart> GetParents(TermPart term);
        IEnumerable<TermPart> GetChildren(TermPart term);
        IEnumerable<TermPart> GetChildren(TermPart term, bool includeParent);
        IEnumerable<IContent> GetContentItems(TermPart term, int skip = 0, int count = 0, string fieldName = null);
        long GetContentItemsCount(TermPart term, string fieldName = null);
        IContentQuery<TermsPart, TermsPartRecord> GetContentItemsQuery(TermPart term, string fieldName = null);

        /// <summary>
        /// Returns all the slugs which can reach a taxonomy
        /// </summary>
        IEnumerable<string> GetSlugs();

        /// <summary>
        /// Organizes a list of <see cref="TermPart"/> objects into a hierarchy.
        /// </summary>
        /// <param name="terms">The <see cref="TermPart"/> objects to orgnanize in a hierarchy. The objects need to be sorted.</param>
        /// <param name="append">The action to perform when a node is added as a child, or <c>null</c> if nothing needs to be done.</param>
        void CreateHierarchy(IEnumerable<TermPart> terms, Action<TermPartNode, TermPartNode> append);
    }
}