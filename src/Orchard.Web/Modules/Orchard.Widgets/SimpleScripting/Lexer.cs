using System.Collections.Generic;

namespace Orchard.Widgets.SimpleScripting {
    public class Lexer {
        private readonly Tokenizer _tokenizer;
        private readonly List<Terminal> _tokens= new List<Terminal>();
        private int _tokenIndex;

        public Lexer(Tokenizer tokenizer) {
            _tokenizer = tokenizer;
        }

        public Terminal Token() {
            if (_tokenIndex == _tokens.Count) {
                _tokens.Add(_tokenizer.NextToken());
            }
            return _tokens[_tokenIndex];
        }

        public void NextToken() {
            _tokenIndex++;
        }

        public Marker Mark() {
            return new Marker(_tokens.Count);
        }

        public void Mark(Marker marker) {
            _tokenIndex = marker.TokenIndex;
        }

        public struct Marker {
            private readonly int _tokenIndex;

            public Marker(int tokenIndex) {
                _tokenIndex = tokenIndex;
            }

            public int TokenIndex {
                get { return _tokenIndex; }
            }
        }
    }
}