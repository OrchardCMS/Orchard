using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Title.Models;

namespace Orchard.Taxonomies.Models {
    public class TermPart : ContentPart<TermPartRecord> {
        public string Name {
            get { return this.As<TitlePart>().Title; }
            set { this.As<TitlePart>().Title = value; }
        }

        public string Slug {
            get { return this.As<AutoroutePart>().DisplayAlias; }
            set { this.As<AutoroutePart>().DisplayAlias = value; }
        }

        public IContent Container {
            get { return this.As<ICommonPart>().Container; }
            set { this.As<ICommonPart>().Container = value; }
        }

        public int TaxonomyId {
            get { return Retrieve(x => x.TaxonomyId); }
            set { Store(x => x.TaxonomyId, value); }
        }

        /// <summary>
        /// e.g., /; /1/; /1/2/
        /// </summary>
        public string Path {
            get { return Retrieve(x => x.Path); }
            set { Store(x => x.Path, value); }
        }

        public int Count {
            get { return Retrieve(x => x.Count); }
            set { Store(x => x.Count, value); }
        }

        public bool Selectable {
            get { return Retrieve(x => x.Selectable); }
            set { Store(x => x.Selectable, value); }
        }

        public int Weight {
            get { return Retrieve(x => x.Weight); }
            set { Store(x => x.Weight, value); }
        }

        public string FullPath { get { return String.Concat(Path, Id); } }

        public static IEnumerable<TermPart> Sort(IEnumerable<TermPart> terms) {
            var list = terms.ToList();
            var index = list.ToDictionary(x => x.FullPath);
            return list.OrderBy(x => x, new TermsComparer(index));
        }

        private class TermsComparer : IComparer<TermPart> {
            private readonly IDictionary<string, TermPart> _index;

            public TermsComparer(IDictionary<string, TermPart> index) {
                _index = index;
            }

            public int Compare(TermPart x, TermPart y) {

                // if two nodes have the same parent, then compare by weight, then by path 
                // /1/2/3 vs /1/2/4 => 3 vs 4
                if (x.Path == y.Path) {
                    var weight = y.Weight.CompareTo(x.Weight);

                    if (weight != 0) {
                        return weight;
                    }

                    // if same parent path and same weight, compare by name
                    return String.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
                } 

                // if two nodes have different parents

                //    if the two nodes have the same root, the deeper is after (i.e. one starts with the other)
                //    /1/2 vs /1/2/3 => /1/2 first

                if (x.FullPath.StartsWith(y.FullPath, StringComparison.OrdinalIgnoreCase)) {
                    return 1;
                }

                if (y.FullPath.StartsWith(x.FullPath, StringComparison.OrdinalIgnoreCase)) {
                    return -1;
                }

                //    otherwise compare first none matching parent
                //    /1/2 vs /1/3 => 2 vs 3
                //    /2/3 vs /4 => 2 vs 4

                var xPath = x.FullPath.Split(new [] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                var yPath = y.FullPath.Split(new [] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                string xFullPath = "", yFullPath = ""; 

                for(var i=0; i< Math.Min(xPath.Length, yPath.Length); i++) {
                    xFullPath += "/" + xPath[i];
                    yFullPath += "/" + yPath[i];

                    if (!xFullPath.Equals(yFullPath, StringComparison.OrdinalIgnoreCase)) {
                        var xParent = _index[xFullPath];
                        var yParent = _index[yFullPath];

                        return Compare(xParent, yParent);
                    }
                }

                return 0;
            }
        }
    }
}