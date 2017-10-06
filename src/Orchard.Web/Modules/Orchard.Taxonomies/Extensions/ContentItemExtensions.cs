using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;

namespace Orchard.ContentManagement
{
    public static class ContentItemExtensions
    {
        public static EagerlyLoadQueryResult<T> LoadTaxonomyFields<T>(this IList<T> items, IContentManager contentManager, bool loadTermsContainter) where T : class, IContent {
            var eagerlyLoadQueryResult = new EagerlyLoadQueryResult<T>(items, contentManager);
            return eagerlyLoadQueryResult.IncludeTaxonomyFields(loadTermsContainter);
        }

        public static EagerlyLoadQueryResult<T> IncludeTaxonomyFields<T>(this IContentQuery<T> query, bool loadTermsContainter) where T : class, IContent {
            var manager = query.ContentManager;
            query = query.Join<TermsPartRecord>().WithQueryHints(new QueryHints().ExpandRecords("TermsPartRecord.Terms"));

            var eagerlyLoadQueryResult = new EagerlyLoadQueryResult<T>(query.List(), manager);

            return eagerlyLoadQueryResult.IncludeTaxonomyFields(loadTermsContainter);
        }

        public static EagerlyLoadQueryResult<T> IncludeTaxonomyFields<T>(this EagerlyLoadQueryResult<T> eagerlyLoadQueryResult, bool loadTermsContainter) where T : class, IContent {
            var contentManager = eagerlyLoadQueryResult.ContentManager as DefaultContentManager;
            var session = contentManager.TransactionManager.GetSession();

            Dictionary<int, Dictionary<int, string>> termsTermRecordIdsDictionary = new Dictionary<int, Dictionary<int, string>>();
            var termsIds = new HashSet<int>();
            List<Object[]> queryResult = new List<Object[]>();
            int pageSize = 2000;
            var itemsCount = eagerlyLoadQueryResult.Result.Count();
            var pagesCount = (itemsCount + pageSize - 1) / pageSize;


            for (var page = 0; page < pagesCount; page++) {
                var objectsToLoad = eagerlyLoadQueryResult.Result.Select(c => c.As<TermsPart>()).Where(t => t != null).ToList();
                if (!objectsToLoad.Any()) {
                    continue;
                }

                StringBuilder sb = new StringBuilder();

                sb.Append("SELECT tc.TermsPartRecord.Id,tc.TermRecord.id,tc.Field FROM Orchard.Taxonomies.Models.TermContentItem as tc ");
                sb.Append("JOIN tc.TermRecord as tp WHERE ");
                var count = 0;
                foreach (var part in objectsToLoad) {
                    sb.Append("tc.TermsPartRecord.id = " + part.Id.ToString());
                    count++;
                    if (count < objectsToLoad.Count) {
                        sb.Append(" OR ");
                    }
                }

                var result = session.CreateQuery(sb.ToString());
                queryResult = result.List<object[]>().ToList();
                foreach (var keyValue in queryResult) {
                    var termRecordId = (int)keyValue[1];
                    if (!termsIds.Contains(termRecordId)) {
                        termsIds.Add(termRecordId);
                    }

                    if (termsTermRecordIdsDictionary.ContainsKey((int)keyValue[0])) {
                        termsTermRecordIdsDictionary[(int)keyValue[0]].Add(termRecordId, (string)keyValue[2]);
                    }
                    else {
                        Dictionary<int, string> TermsRecordFieldDictionary = new Dictionary<int, string>();
                        TermsRecordFieldDictionary.Add(termRecordId, (string)keyValue[2]);
                        termsTermRecordIdsDictionary.Add((int)keyValue[0], TermsRecordFieldDictionary);
                    }
                }

                var termsDictionary = eagerlyLoadQueryResult.ContentManager.GetTooMany<TermPart>(termsIds, VersionOptions.Published
                      , new QueryHints().ExpandRecords("ContentTypeRecord", "CommonPartRecord", "TermsPartRecord"))
                     .ToDictionary(c => c.ContentItem.Id);

                foreach (var resultPart in objectsToLoad) {
                    var fields = resultPart.ContentItem.Parts.SelectMany(p => p.Fields.OfType<TaxonomyField>());
                    var preloadedTerms = new List<TermContentItemPart>();
                    foreach (var field in fields) {
                        var preloadedFieldParts = new List<TermPart>();
                        TermPart preloadedPart = null;
                        Dictionary<int, string> TermsRecordFieldDictionary;
                        if (termsTermRecordIdsDictionary.TryGetValue(resultPart.Id, out TermsRecordFieldDictionary)) {
                            foreach (var fieldWithTerm in TermsRecordFieldDictionary) {
                                if (fieldWithTerm.Value == field.Name && termsDictionary.TryGetValue(fieldWithTerm.Key, out preloadedPart)) {
                                    preloadedFieldParts.Add(preloadedPart);
                                    preloadedTerms.Add(new TermContentItemPart {
                                        Field = field.Name,
                                        TermPart = preloadedPart
                                    });
                                }
                            }
                            field.Terms = preloadedFieldParts;
                        }
                        resultPart.TermParts = preloadedTerms;
                    }
                }
                if (loadTermsContainter && termsDictionary.Any()) {
                    var pendingResults = new EagerlyLoadQueryResult<IContent>(termsDictionary.Values, eagerlyLoadQueryResult.ContentManager);
                    pendingResults.IncludeContainerContentItems(1);
                }
            }

            return eagerlyLoadQueryResult;
        }
    }
}

