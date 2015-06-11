using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Routing;
using Orchard.Alias.Implementation.Holder;
using System.Collections.Concurrent;

namespace Orchard.Alias.Implementation.Map {
    public class AliasMap {
        private readonly string _area;
        private readonly ConcurrentDictionary<string, IDictionary<string, string>> _aliases;
        private readonly Node _root;

        public AliasMap(string area) {
            _area = area;
            _aliases = new ConcurrentDictionary<string, IDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            _root = new Node();
        }

        public IEnumerable<AliasInfo> GetAliases() {
            return _aliases.Select(x => new AliasInfo {Area = _area, Path = x.Key, RouteValues = x.Value});
        }
 
        public bool TryGetAlias(string virtualPath, out IDictionary<string, string> routeValues) {
            return _aliases.TryGetValue(virtualPath, out routeValues);
        }

        public Tuple<IDictionary<string, object>, string> Locate(RouteValueDictionary routeValues) {
            return Traverse(_root, routeValues, _area);
        }

        /// <summary>
        /// Adds an <see cref="AliasInfo"/> to the map
        /// </summary>
        /// <param name="info">The <see cref="AliasInfo"/> intance to add</param>
        public void Insert(AliasInfo info) {
            if(info == null) {
                throw new ArgumentNullException();
            }

            _aliases[info.Path] = info.RouteValues;
            ExpandTree(_root, info.Path, info.RouteValues);
        }

        /// <summary>
        /// Removes an alias from the map
        /// </summary>
        /// <param name="info"></param>
        public void Remove(AliasInfo info) {
            IDictionary<string,string> values;
            _aliases.TryRemove(info.Path, out values);
            CollapseTree(_root, info.Path, info.RouteValues);
        }

        public bool Any() {
            return _aliases.Any();
        }

        private static void CollapseTree(Node root, string path, IDictionary<string, string> routeValues) {
            foreach (var expanded in Expand(routeValues)) {
                var focus = root;
                foreach (var routeValue in expanded.OrderBy(kv => kv.Key, StringComparer.InvariantCultureIgnoreCase)) {
                    // See if we already have a stem for this route key (i.e. "controller") and create if not
                    var stem = focus.Stems.GetOrAdd(routeValue.Key, key => new ConcurrentDictionary<string, Node>(StringComparer.InvariantCultureIgnoreCase));
                    // See if the stem has a node for this value (i.e. "Item") and create if not
                    var node = stem.GetOrAdd(routeValue.Value, key => new Node());
                    // Keep switching to new node until we reach deepest match
                    // TODO: (PH) Thread safety: at this point something could techincally traverse and find an empty node with a blank path ... not fatal
                    // since it will simply not match and therefore return a default-looking route instead of the aliased one. And the changes of that route
                    // being the same one which is just being added are very low.
                    focus = node;
                }
                // Set the path at the end of the tree
                object takenPath;
                focus.Paths.TryRemove(path,out takenPath);
            }
        }

        private static void ExpandTree(Node root, string path, IDictionary<string, string> routeValues) {
            foreach(var expanded in Expand(routeValues)) {
                var focus = root;
                foreach (var routeValue in expanded.OrderBy(kv => kv.Key, StringComparer.InvariantCultureIgnoreCase)) {
                    // See if we already have a stem for this route key (i.e. "controller") and create if not
                    var stem = focus.Stems.GetOrAdd(routeValue.Key,key=>new ConcurrentDictionary<string, Node>(StringComparer.InvariantCultureIgnoreCase));
                    // See if the stem has a node for this value (i.e. "Item") and create if not
                    var node = stem.GetOrAdd(routeValue.Value, key=>new Node());
                    // Keep switching to new node until we reach deepest match
                    // TODO: (PH) Thread safety: at this point something could techincally traverse and find an empty node with a blank path ... not fatal
                    // since it will simply not match and therefore return a default-looking route instead of the aliased one. And the changes of that route
                    // being the same one which is just being added are very low.
                    focus = node;
                }
                // Set the path at the end of the tree
                focus.Paths.TryAdd(path, null);
            }
        }

        private static IEnumerable<TResult> Product<T1, T2, TResult>(IEnumerable<T1> source1, IEnumerable<T2> source2, Func<T1, T2, TResult> produce) {
            return from item1 in source1 from item2 in source2 select produce(item1, item2);
        }

        /// <summary>
        /// Expand the route values into all possible combinations of keys
        /// </summary>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        private static IEnumerable<IEnumerable<KeyValuePair<string, string>>> Expand(IDictionary<string, string> routeValues) {
            var ordered = routeValues.OrderBy(kv => kv.Key, StringComparer.InvariantCultureIgnoreCase);
            var empty = Enumerable.Empty<KeyValuePair<string, string>>();

            // For each key/value pair, we want a list containing a single list with either the term, or the term and the "default" value
            var termSets = ordered.Select(term => {
                                              if (term.Key.EndsWith("-")) {
                                                  var termKey = term.Key.Substring(0, term.Key.Length - 1);
                                                  return new[] {
                                                      // This entry will auto-match in some cases because it was omitted from the route values
                                                      new [] { new KeyValuePair<string, string>(termKey, "\u0000") },
                                                      new [] { new KeyValuePair<string, string>(termKey, term.Value) }
                                                  };
                                              }
                                              return new[] {new[] {term}};
                                          });

            // Run each of those lists through an aggregation function, by taking the product of each set, so producting a tree of possibilities
            var produced = termSets.Aggregate(new[] { empty }.AsEnumerable(), (coords, termSet) => Product(coords, termSet, (coord, term) => coord.Concat(term)));
            return produced;
        }


        private static Tuple<IDictionary<string, object>, string> Traverse(Node focus, RouteValueDictionary routeValues, string areaName) {

            // Initialize a match variable
            Tuple<IDictionary<string, object>, string> match = null;

            // Check each stem to try and find a match for all the route values
            // TODO: (PH) We know it's concurrent; but what happens if a new element is added during this loop?
            // TODO: (PH) Also, this could be optimised more. Need to see how many stems are typically being looped - could arrange things for a
            // much quicker matchup against the routeValues and using the fact that the stems have keys.
            foreach (var stem in focus.Stems) {
                var routeValue = "\u0000"; // Represents the default value when not provided in the route values

                object value;
                // Area has been stripped out of routeValues (because the route is IRouteWithArea MVC assumes the area is inferred, I think)
                // but we still need to match it in the stem (another way would be to strip it out of the nodes altogether). PH
                // TODO: Actually was this supposed to be the behaviour of the hyphens; by adding the hyphen MVC will no longer strip it out? PH
                if (stem.Key == "area") {
                    routeValue = areaName;
                }
                // See if the route we're checking contains the key
                if (routeValues.TryGetValue(stem.Key, out value)) {
                    routeValue = Convert.ToString(value, CultureInfo.InvariantCulture);
                }

                // Must find a value on the stem, matching the route's value
                Node node;
                if (!stem.Value.TryGetValue(routeValue, out node)) {
                    continue;
                }

                // Continue traversing with the new node
                var deeper = Traverse(node, routeValues, areaName);
                if (deeper == null) {
                    continue;
                }

                // Create a key in the dictionary
                deeper.Item1.Add(stem.Key, null);
                // If it's better than a current match (more items), take it
                if (match == null || deeper.Item1.Count > match.Item1.Count) {
                    match = deeper;
                }
            }

            if (match == null) {
                var foundPath = focus.Paths.Keys.FirstOrDefault();
                if (foundPath != null) {
                    // Here the deepest match is being created, which will be populated as it rises back up the stack, but save the path here.
                    // Within this function it's used to count how many items match so we get the best one; but when it's returned
                    // to AliasRoute it will also need the key lookup for speed
                    match = Tuple.Create((IDictionary<string, object>)new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase), foundPath);
                }
            }
            return match;
        }

        private class Node {
            public Node() {
                Stems = new ConcurrentDictionary<string, ConcurrentDictionary<string, Node>>(StringComparer.InvariantCultureIgnoreCase);
                Paths = new ConcurrentDictionary<string, object>();
            }

            public ConcurrentDictionary<string, ConcurrentDictionary<string, Node>> Stems { get; set; }
            public ConcurrentDictionary<string,object> Paths { get; set; }
        }

    }
}