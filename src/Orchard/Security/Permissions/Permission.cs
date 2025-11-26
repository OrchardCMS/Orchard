using System;
using System.Collections.Generic;
using Orchard.Localization;

namespace Orchard.Security.Permissions
{
    public class Permission
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }

        public IEnumerable<Permission> ImpliedBy { get; set; }

        /// <summary>
        /// Gets a value indicating whether the permission is security critical.
        /// </summary>
        public bool IsSecurityCritical { get; set; }

        public LocalizedString Hint { get; set; }

        public static Permission Named(string name)
        {
            return new Permission { Name = name };
        }

        [Obsolete("This property is not used anywhere, so it shouldn't be referenced.")]
        public bool RequiresOwnership { get; set; }
    }
}
