using Orchard.Scripting.Compiler;

namespace Orchard.Scripting.Ast {
    public interface IAstNodeWithToken {
        Token Token { get; }
    }
}