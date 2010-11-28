using System.Collections.Generic;

namespace Orchard.Widgets.SimpleScripting {
    public class ExpressionLexer {
        private readonly ExpressionTokenizer _tokenizer;
        private readonly List<ExpressionTokenizer.Token> _tokens= new List<ExpressionTokenizer.Token>();
        private int _tokenIndex;

        public ExpressionLexer(ExpressionTokenizer tokenizer) {
            _tokenizer = tokenizer;
        }

        public ExpressionTokenizer.Token Token() {
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