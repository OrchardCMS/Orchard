using System;
using System.Reflection;
using Orchard.Localization;

namespace Orchard.Commands {
    public abstract class DefaultOrchardCommandHandler : ICommandHandler {
        protected DefaultOrchardCommandHandler() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        #region Implementation of ICommandHandler

        public void Execute(CommandContext context) {
            MethodInfo methodInfo = GetType().GetMethod(context.Command);
            if (methodInfo != null) {
                context.Output = (string) methodInfo.Invoke(this, null);
            }
            else {
                throw new InvalidOperationException(T("Command : ") + context.Command + T(" was not found"));
            }
        }

        #endregion
    }
}
