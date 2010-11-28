namespace Orchard.Widgets.SimpleScripting {
    public enum TokenKind {
        Eof,
        OpenParen,
        CloseParen,
        StringLiteral,
        SingleQuotedStringLiteral,
        NumberLiteral,
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