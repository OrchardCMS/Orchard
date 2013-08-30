using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.UI.Navigation;
using Orchard.Taxonomies.Helpers;


namespace Orchard.Taxonomies.Navigation {
    /// <summary>
    /// Dynamically injects query results as menu items on NavigationQueryMenuItem elements
    /// </summary>
    public class TaxonomyNavigationProvider : INavigationFilter {
        private readonly IContentManager _contentManager;
        private readonly ITaxonomyService _taxonomyService;

        public TaxonomyNavigationProvider(
            IContentManager contentManager,
            ITaxonomyService taxonomyService) {
            _contentManager = contentManager;
            _taxonomyService = taxonomyService;
        }

        public IEnumerable<MenuItem> Filter(IEnumerable<MenuItem> items) {

            foreach (var item in items) {
                if (item.Content != null && item.Content.ContentItem.ContentType == "TaxonomyNavigationMenuItem") {
                    // expand query

                    var taxonomyNavigationPart = item.Content.As<TaxonomyNavigationPart>();

                    var rootTerm = _taxonomyService.GetTerm(taxonomyNavigationPart.TermId);

                    List<int> positionList = new List<int>();

                    var allTerms = rootTerm != null
                                       ? _taxonomyService.GetChildren(rootTerm).ToArray()
                                       : _taxonomyService.GetTerms(taxonomyNavigationPart.TaxonomyId).ToArray();

                    var rootlevel = rootTerm == null ? 0 : rootTerm.GetLevels();
                    
                    positionList.Add(0);

                    var menuPosition = item.Position;
                    int parentLevel = rootlevel;

                    foreach (var contentItem in allTerms) {
                        if (contentItem != null) {
                            var part = contentItem;

                            if (taxonomyNavigationPart.HideEmptyTerms == true && part.Count == 0) {
                                continue;
                            }
                            string termPosition = "";
                            if (part.GetLevels() - rootlevel > parentLevel) {
                                positionList.Add(0);
                                parentLevel = positionList.Count - 1;
                            }
                            else
                                if ((part.GetLevels() - rootlevel) == parentLevel) {
                                    positionList[parentLevel]++;
                                }
                                else {
                                    positionList.RemoveRange(1, positionList.Count - 1);
                                    parentLevel = positionList.Count - 1;
                                    positionList[parentLevel]++;
                                }

                            termPosition = positionList.First().ToString();
                            foreach (var position in positionList.Skip(1)) {
                                termPosition = termPosition + "." + position.ToString();
                            }


                            var menuText = _contentManager.GetItemMetadata(part).DisplayText;
                            var routes = _contentManager.GetItemMetadata(part).DisplayRouteValues;

                            if (taxonomyNavigationPart.DisplayContentCount) {
                                menuText = String.Format(menuText + " ({0})", part.Count.ToString());
                            }

                            var inserted = new MenuItem {
                                Text = new LocalizedString(menuText),
                                IdHint = item.IdHint,
                                Classes = item.Classes,
                                Url = item.Url,
                                Href = item.Href,
                                LinkToFirstChild = false,
                                RouteValues = routes,
                                LocalNav = item.LocalNav,
                                Items = new MenuItem[0],
                                Position = menuPosition + ":" + termPosition,
                                Permissions = item.Permissions,
                                Content = part
                            };

                            yield return inserted;
                        }
                    }
                }
                else {
                    yield return item;
                }
            }
        }
    }
}