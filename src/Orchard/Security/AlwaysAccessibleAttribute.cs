using System;

namespace Orchard.Security {
    /// <summary>
    /// Applied on a Controller or an Action, it will prevent any action from being filtered by the AccessFrontEnd permission.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AlwaysAccessibleAttribute : Attribute {
    }
}
