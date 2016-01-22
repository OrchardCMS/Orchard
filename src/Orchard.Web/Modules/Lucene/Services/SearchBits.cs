using System;
using Lucene.Net.Util;
using Orchard.Indexing;

namespace Lucene.Services {
    public class SearchBits : ISearchBits {
        internal readonly OpenBitSet _openBitSet;

        public SearchBits(OpenBitSet openBitSet) {
            _openBitSet = openBitSet;
        }

        public ISearchBits And(ISearchBits other) {
            return Apply(other, (x, y) => x.And(y));
        }

        public ISearchBits Or(ISearchBits other) {
            return Apply(other, (x, y) => x.Or(y));
        }

        public ISearchBits Xor(ISearchBits other) {
            return Apply(other, (x, y) => x.Xor(y));
        }

        public long Count() {
            return _openBitSet.Cardinality();
        }

        private ISearchBits Apply(ISearchBits other, Action<OpenBitSet, OpenBitSet> operation) {
            var bitset = (OpenBitSet)_openBitSet.Clone();
            var otherBitSet = other as SearchBits;

            if (otherBitSet == null) {
                throw new InvalidOperationException("The other bitset must be of type OpenBitSet");
            }

            operation(bitset, otherBitSet._openBitSet);

            return new SearchBits(bitset);

        }
    }
}