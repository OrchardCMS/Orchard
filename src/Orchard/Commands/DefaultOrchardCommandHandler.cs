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
            foreach (MethodInfo methodInfo in GetType().GetMethods()) {
                if (String.Equals(methodInfo.Name, context.Command, StringComparison.OrdinalIgnoreCase)) {
                    context.Output = (string)methodInfo.Invoke(this, null);
                    return;
                }

                foreach (OrchardCommandAttribute commandAttribute in methodInfo.GetCustomAttributes(typeof(OrchardCommandAttribute), false)) {
                    if (String.Equals(commandAttribute.Command, context.Command, StringComparison.OrdinalIgnoreCase)) {
                        context.Output = (string)methodInfo.Invoke(this, null);
                        return;
                    }
                }
            }

            throw new InvalidOperationException(T("Command : ") + context.Command + T(" was not found"));
        }

        #endregion
    }
}
