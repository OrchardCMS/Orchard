namespace Orchard.Widgets.SimpleScripting {
    public enum TerminalKind {
        Eof,
        OpenParen,
        CloseParen,
        StringLiteral,
        SingleQuotedStringLiteral,
        Integer,
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