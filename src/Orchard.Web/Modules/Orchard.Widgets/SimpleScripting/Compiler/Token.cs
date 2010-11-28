using System;

namespace Orchard.Widgets.SimpleScripting.Compiler {
    public class Token {
        public TokenKind Kind { get; set; }
        public int Position { get; set; }
        public object Value { get; set; }

        public override string ToString() {
            return String.Format("{0} ({1}) at position {2}", Kind, Value ?? "<noval>", Position);
        }
    }
}