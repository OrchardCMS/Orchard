using System;

namespace Orchard.Security {
    /// <summary>
    /// Applied on a Controller or an Action, will prevent any action from being filtered by AccessFrontEnd permssion
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AlwaysAccessibleAttribute : Attribute {
    }
}
