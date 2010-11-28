namespace Orchard.Widgets.SimpleScripting.Compiler {
    public enum TokenKind {
        Eof,
        OpenParen,
        CloseParen,
        StringLiteral,
        SingleQuotedStringLiteral,
        Integer,
        Comma,
        Plus,
        Minus,
        Mul,
        Div,
        True,
        False,
        Identifier,
        And,
        Or,
        Not,
        Invalid
    }
}