using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using Orchard.Projections.Descriptors;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Property;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Projections.Descriptors.Layout;
using Orchard.Projections.Descriptors.SortCriterion;
using Orchard.Projections.Models;
using Orchard.Tokens;

namespace Orchard.Projections.Services {
    public class ProjectionManager : IProjectionManager{
        private readonly ITokenizer _tokenizer;
        private readonly IEnumerable<IFilterProvider> _filterProviders;
        private readonly IEnumerable<ISortCriterionProvider> _sortCriterionProviders;
        private readonly IEnumerable<ILayoutProvider> _layoutProviders;
        private readonly IEnumerable<IPropertyProvider> _propertyProviders;
        private readonly IContentManager _contentManager;
        private readonly IRepository<QueryPartRecord> _queryRepository;

        public ProjectionManager(
            ITokenizer tokenizer,
            IEnumerable<IFilterProvider> filterProviders,
            IEnumerable<ISortCriterionProvider> sortCriterionProviders,
            IEnumerable<ILayoutProvider> layoutProviders,
            IEnumerable<IPropertyProvider> propertyProviders,
            IContentManager contentManager,
            IRepository<QueryPartRecord> queryRepository) {
            _tokenizer = tokenizer;
            _filterProviders = filterProviders;
            _sortCriterionProviders = sortCriterionProviders;
            _layoutProviders = layoutProviders;
            _propertyProviders = propertyProviders;
            _contentManager = contentManager;
            _queryRepository = queryRepository;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<TypeDescriptor<FilterDescriptor>> DescribeFilters() {
            var context = new DescribeFilterContext();

            foreach (var provider in _filterProviders) {
                provider.Describe(context);
            }
            return context.Describe();
        }

        public IEnumerable<TypeDescriptor<SortCriterionDescriptor>> DescribeSortCriteria() {
            var context = new DescribeSortCriterionContext();

            foreach (var provider in _sortCriterionProviders) {
                provider.Describe(context);
            }
            return context.Describe();
        }

        public IEnumerable<TypeDescriptor<LayoutDescriptor>> DescribeLayouts() {
            var context = new DescribeLayoutContext();

            foreach (var provider in _layoutProviders) {
                provider.Describe(context);
            }
            return context.Describe();
        }

        public IEnumerable<TypeDescriptor<PropertyDescriptor>> DescribeProperties() {
            var context = new DescribePropertyContext();

            foreach (var provider in _propertyProviders) {
                provider.Describe(context);
            }
            return context.Describe();
        }

        public int GetCount(int queryId) {

            var queryRecord = _queryRepository.Get(queryId);

            if (queryRecord == null) {
                throw new ArgumentException("queryId");
            }

            // aggregate the result for each group query

            return GetContentQueries(queryRecord, Enumerable.Empty<SortCriterionRecord>())
                .Sum(contentQuery => contentQuery.Count());
        }

        public IEnumerable<ContentItem> GetContentItems(int queryId, int skip = 0, int count = 0) {
            var availableSortCriteria = DescribeSortCriteria().ToList();

            var queryRecord = _queryRepository.Get(queryId);

            if(queryRecord == null) {
                throw new ArgumentException("queryId");
            }

            var contentItems = new List<ContentItem>();

            // aggregate the result for each group query
            foreach(var contentQuery in GetContentQueries(queryRecord, queryRecord.SortCriteria)) {
                contentItems.AddRange(contentQuery.Slice(skip, count));
            }

            if(queryRecord.FilterGroups.Count <= 1) {
                return contentItems;
            }

            // re-executing the sorting with the cumulated groups
            var ids = contentItems.Select(c => c.Id).ToArray();

            if(ids.Length == 0) {
                return Enumerable.Empty<ContentItem>();
            }

            var groupQuery = _contentManager.HqlQuery().Where(alias => alias.Named("ci"), x => x.InG("Id", ids));

            // iterate over each sort criteria to apply the alterations to the query object
            foreach (var sortCriterion in queryRecord.SortCriteria) {
                var sortCriterionContext = new SortCriterionContext {
                    Query = groupQuery,
                    State = FormParametersHelper.ToDynamic(sortCriterion.State)
                };

                string category = sortCriterion.Category;
                string type = sortCriterion.Type;

                // look for the specific filter component
                var descriptor = availableSortCriteria.SelectMany(x => x.Descriptors).FirstOrDefault(x => x.Category == category && x.Type == type);

                // ignore unfound descriptors
                if (descriptor == null) {
                    continue;
                }

                // apply alteration
                descriptor.Sort(sortCriterionContext);

                groupQuery = sortCriterionContext.Query;
            }

            return groupQuery.Slice(skip, count);
        }

        public IEnumerable<IHqlQuery> GetContentQueries(QueryPartRecord queryRecord, IEnumerable<SortCriterionRecord> sortCriteria) {
            var availableFilters = DescribeFilters().ToList();
            var availableSortCriteria = DescribeSortCriteria().ToList();

            // pre-executing all groups 
            foreach (var group in queryRecord.FilterGroups) {

                var contentQuery = _contentManager.HqlQuery().ForVersion(VersionOptions.Published);

                // iterate over each filter to apply the alterations to the query object
                foreach (var filter in group.Filters) {
                    var tokenizedState = _tokenizer.Replace(filter.State, new Dictionary<string, object>());
                    var filterContext = new FilterContext {
                        Query = contentQuery,
                        State = FormParametersHelper.ToDynamic(tokenizedState)
                    };

                    string category = filter.Category;
                    string type = filter.Type;

                    // look for the specific filter component
                    var descriptor = availableFilters
                        .SelectMany(x => x.Descriptors)
                        .FirstOrDefault(x => x.Category == category && x.Type == type);

                    // ignore unfound descriptors
                    if (descriptor == null) {
                        continue;
                    }

                    // apply alteration
                    descriptor.Filter(filterContext);

                    contentQuery = filterContext.Query;
                }

                // iterate over each sort criteria to apply the alterations to the query object
                foreach (var sortCriterion in sortCriteria) {
                    var sortCriterionContext = new SortCriterionContext {
                        Query = contentQuery,
                        State = FormParametersHelper.ToDynamic(sortCriterion.State)
                    };

                    string category = sortCriterion.Category;
                    string type = sortCriterion.Type;

                    // look for the specific filter component
                    var descriptor = availableSortCriteria
                        .SelectMany(x => x.Descriptors)
                        .FirstOrDefault(x => x.Category == category && x.Type == type);

                    // ignore unfound descriptors
                    if (descriptor == null) {
                        continue;
                    }

                    // apply alteration
                    descriptor.Sort(sortCriterionContext);

                    contentQuery = sortCriterionContext.Query;
                }


                yield return contentQuery;
            }            
        }
    }
}