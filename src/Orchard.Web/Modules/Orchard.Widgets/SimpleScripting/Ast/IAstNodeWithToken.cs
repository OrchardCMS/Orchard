using Orchard.Widgets.SimpleScripting.Compiler;

namespace Orchard.Widgets.SimpleScripting.Ast {
    public interface IAstNodeWithToken {
        Token Token { get; }
    }
}