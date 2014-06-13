using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Autoroute.Models;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Tags.Models;

namespace Orchard.Tags.Services {
    [OrchardFeature("Orchard.Tags.TagCloud")]
    public class TagCloudService : ITagCloudService {
        private readonly IRepository<ContentTagRecord> _contentTagRepository;
        private readonly IRepository<AutoroutePartRecord> _autorouteRepository;
        private readonly IContentManager _contentManager;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        internal static readonly string TagCloudTagsChanged = "Orchard.Tags.TagCloud.TagsChanged";

        public TagCloudService(
            IRepository<ContentTagRecord> contentTagRepository,
            IRepository<AutoroutePartRecord> autorouteRepository,
            IContentManager contentManager,
            ICacheManager cacheManager,
            ISignals signals) {

            _contentTagRepository = contentTagRepository;
            _autorouteRepository = autorouteRepository;
            _contentManager = contentManager;
            _cacheManager = cacheManager;
            _signals = signals;
        }

        public IEnumerable<TagCount> GetPopularTags(int buckets, string slug) {
            var cacheKey = "Orchard.Tags.TagCloud." + (slug ?? "") + '.' + buckets;
            return _cacheManager.Get(cacheKey,
                ctx => {
                    ctx.Monitor(_signals.When(TagCloudTagsChanged));
                    IEnumerable<TagCount> tagCounts;
                    if (string.IsNullOrWhiteSpace(slug)) {
                        tagCounts = (from tc in _contentTagRepository.Table
                            where tc.TagsPartRecord.ContentItemRecord.Versions.Any(v => v.Published)
                            group tc by tc.TagRecord.TagName
                            into g
                            select new TagCount {
                                TagName = g.Key,
                                Count = g.Count()
                            }).ToList();
                    }
                    else {
                        if (slug == "/") {
                            slug = "";
                        }

                        var containerId = _autorouteRepository.Table
                            .Where(c => c.DisplayAlias == slug)
                            .Select(x => x.Id)
                            .ToList() // don't try to optimize with slicing  as there should be only one result
                            .FirstOrDefault();

                        if (containerId == 0) {
                            return new List<TagCount>();
                        }

                        tagCounts = _contentManager
                                          .Query<TagsPart, TagsPartRecord>(VersionOptions.Published)
                                          .Join<CommonPartRecord>()
                                          .Where(t => t.Container.Id == containerId)
                                          .List()
                                          .SelectMany(t => t.CurrentTags)
                                          .GroupBy(t => t)
                                          .Select(g => new TagCount {
                                              TagName = g.Key,
                                              Count = g.Count()
                                          })
                                          .ToList();

                        if (!tagCounts.Any()) {
                            return new List<TagCount>();
                        }
                    }

                    // initialize centroids with a linear distribution
                    var centroids = new int[buckets];
                    var maxCount = tagCounts.Any() ? tagCounts.Max(tc => tc.Count) : 0;
                    var minCount = tagCounts.Any() ? tagCounts.Min(tc => tc.Count) : 0;
                    var maxDistance = maxCount - minCount;
                    for (int i = 0; i < centroids.Length; i++) {
                        centroids[i] = maxDistance/buckets * (i+1);
                    }

                    var balanced = false;
                    var loops = 0;

                    // loop until equilibrium or instability
                    while (!balanced && loops++ < 50) {
                        balanced = true;
                        // assign to closest buckets
                        foreach (var tagCount in tagCounts) {
                            // look for closest bucket
                            var currentDistance = Math.Abs(tagCount.Count - centroids[tagCount.Bucket - 1]);
                            for(int i=0; i<buckets; i++) {
                                var distance = Math.Abs(tagCount.Count - centroids[i]);
                                if (distance < currentDistance) {
                                    tagCount.Bucket = i + 1;
                                    currentDistance = distance;
                                    balanced = false;
                                }
                            }
                        }

                        // recalculate centroids
                        for (int i = 0; i < buckets; i++) {
                            var target = tagCounts.Where(x => x.Bucket == i + 1).ToArray();
                            if (target.Any()) {
                                centroids[i] = (int)target.Average(x => x.Count);
                            }
                        }
                    }

                    return tagCounts;
                });
        }
    }
}