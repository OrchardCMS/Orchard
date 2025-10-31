using System;

namespace Orchard.Commands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandNameAttribute : Attribute
    {
        public CommandNameAttribute(string commandAlias)
        {
            Command = commandAlias;
        }

        public string Command { get; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CommandHelpAttribute : Attribute
    {
        public CommandHelpAttribute(string text)
        {
            this.HelpText = text;
        }

        public string HelpText { get; set; }
    }
}
