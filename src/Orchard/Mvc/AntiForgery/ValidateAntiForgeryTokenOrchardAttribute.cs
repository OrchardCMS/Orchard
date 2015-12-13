using System;
using System.Web.Mvc;

namespace Orchard.Mvc.AntiForgery {
    [AttributeUsage(AttributeTargets.Method)]
    public class ValidateAntiForgeryTokenOrchardAttribute : FilterAttribute {
        private readonly bool _enabled = true;

        public ValidateAntiForgeryTokenOrchardAttribute() : this(true) {}

        public ValidateAntiForgeryTokenOrchardAttribute(bool enabled) {
            _enabled = enabled;
        }

        public bool Enabled { get { return _enabled; } }
    }
}