using System;
using System.Text;

namespace Orchard.Widgets.SimpleScripting {
    public class Tokenizer {
        private readonly string _expression;
        private readonly StringBuilder _stringBuilder;
        private int _index;
        private int _startTokenIndex;

        public Tokenizer(string expression) {
            _expression = expression;
            _stringBuilder = new StringBuilder();
        }

        public Terminal NextToken() {
            if (Eof())
                return CreateToken(TerminalKind.Eof);

        LexAgain:
            _startTokenIndex = _index;
            char ch = Character();
            switch (ch) {
                case '(':
                    NextCharacter();
                    return CreateToken(TerminalKind.OpenParen);
                case ')':
                    NextCharacter();
                    return CreateToken(TerminalKind.CloseParen);
                case '+':
                    NextCharacter();
                    return CreateToken(TerminalKind.Plus);
                case '-':
                    NextCharacter();
                    return CreateToken(TerminalKind.Minus);
                case '*':
                    NextCharacter();
                    return CreateToken(TerminalKind.Mul);
                case '/':
                    NextCharacter();
                    return CreateToken(TerminalKind.Div);
                case '"':
                    return LexStringLiteral();
                case '\'':
                    return LexSingleQuotedStringLiteral();
            }

            if (IsDigitCharacter(ch)) {
                return LexInteger();
            }
            else if (IsIdentifierCharacter(ch)) {
                return LexIdentifierOrKeyword();
            }
            else if (IsWhitespaceCharacter(ch)) {
                NextCharacter();
                goto LexAgain;
            }

            return CreateToken(TerminalKind.Invalid, "Unrecognized character");
        }

        private Terminal LexIdentifierOrKeyword() {
            _stringBuilder.Clear();

            _stringBuilder.Append(Character());
            while (true) {
                NextCharacter();

                if (!Eof() && (IsIdentifierCharacter(Character()) || IsDigitCharacter(Character()))) {
                    _stringBuilder.Append(Character());
                }
                else {
                    return CreateIdentiferOrKeyword(_stringBuilder.ToString());
                }
            }
        }

        private Terminal LexInteger() {
            _stringBuilder.Clear();

            _stringBuilder.Append(Character());
            while (true) {
                NextCharacter();

                if (!Eof() && IsDigitCharacter(Character())) {
                    _stringBuilder.Append(Character());
                }
                else {
                    return CreateToken(TerminalKind.Integer, Int32.Parse(_stringBuilder.ToString()));
                }
            }
        }

        private Terminal CreateIdentiferOrKeyword(string identifier) {
            switch (identifier) {
                case "true":
                    return CreateToken(TerminalKind.True, true);
                case "false":
                    return CreateToken(TerminalKind.False, false);
                case "or":
                    return CreateToken(TerminalKind.Or, null);
                case "and":
                    return CreateToken(TerminalKind.And, null);
                case "not":
                    return CreateToken(TerminalKind.Not, null);
                default:
                    return CreateToken(TerminalKind.Identifier, identifier);
            }
        }

        private bool IsWhitespaceCharacter(char character) {
            return char.IsWhiteSpace(character);
        }

        private bool IsIdentifierCharacter(char ch) {
            return
                (ch >= 'a' && ch <= 'z') ||
                (ch >= 'A' && ch <= 'Z') ||
                (ch == '_');
        }

        private bool IsDigitCharacter(char ch) {
            return ch >= '0' && ch <= '9';
        }

        private Terminal LexSingleQuotedStringLiteral() {
            _stringBuilder.Clear();

            while (true) {
                NextCharacter();

                if (Eof())
                    return CreateToken(TerminalKind.Invalid, "Unterminated string literal");

                // Termination
                if (Character() == '\'') {
                    NextCharacter();
                    return CreateToken(TerminalKind.SingleQuotedStringLiteral, _stringBuilder.ToString());
                }
                // backslash notation
                else if (Character() == '\\') {
                    NextCharacter();

                    if (Eof())
                        return CreateToken(TerminalKind.Invalid, "Unterminated string literal");

                    if (Character() == '\\') {
                        _stringBuilder.Append('\\');
                    }
                    else if (Character() == '\'') {
                        _stringBuilder.Append('\'');
                    }
                    else {
                        _stringBuilder.Append('\\');
                        _stringBuilder.Append(Character());
                    }
                }
                // Regular character in string
                else {
                    _stringBuilder.Append(Character());
                }
            }
        }

        private Terminal LexStringLiteral() {
            _stringBuilder.Clear();

            while (true) {
                NextCharacter();

                if (Eof())
                    return CreateToken(TerminalKind.Invalid, "Unterminated string literal");

                // Termination
                if (Character() == '"') {
                    NextCharacter();
                    return CreateToken(TerminalKind.StringLiteral, _stringBuilder.ToString());
                }
                // backslash notation
                else if (Character() == '\\') {
                    NextCharacter();

                    if (Eof())
                        return CreateToken(TerminalKind.Invalid, "Unterminated string literal");

                    _stringBuilder.Append(Character());
                }
                // Regular character in string
                else {
                    _stringBuilder.Append(Character());
                }
            }
        }

        private void NextCharacter() {
            _index++;
        }

        private char Character() {
            return _expression[_index];
        }

        private Terminal CreateToken(TerminalKind kind, object value = null) {
            return new Terminal {
                Kind = kind,
                Position = _startTokenIndex,
                Value = value
            };
        }

        private bool Eof() {
            return (_index >= _expression.Length);
        }
    }
}