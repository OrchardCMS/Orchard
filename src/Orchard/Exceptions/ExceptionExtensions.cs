using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using Orchard.Security;

namespace Orchard.Exceptions {
    public static class ExceptionExtensions {
        public static bool IsFatal(this Exception ex) {
            return ex is OrchardSecurityException ||
                ex is StackOverflowException ||
                ex is OutOfMemoryException ||
                ex is AccessViolationException ||
                ex is AppDomainUnloadedException ||
                ex is ThreadAbortException ||
                ex is SecurityException ||
                ex is SEHException;
        }
    }
}
