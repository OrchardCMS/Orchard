using System;
using System.Web.Mvc;

namespace Orchard.Mvc.AntiForgery
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ValidateAntiForgeryTokenOrchardAttribute : FilterAttribute
    {
        public ValidateAntiForgeryTokenOrchardAttribute() : this(true) { }

        public ValidateAntiForgeryTokenOrchardAttribute(bool enabled)
        {
            Enabled = enabled;
        }

        public bool Enabled { get; } = true;
    }
}