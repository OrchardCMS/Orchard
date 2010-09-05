using System;
using System.Collections.Generic;
using System.Web;
using Orchard.DisplayManagement.Implementation;

namespace Orchard.DisplayManagement.Descriptors {
    public class ShapeDescriptor {
        public string ShapeType { get; set; }
        
        /// <summary>
        /// The BindingSource is informational text about the source of the Binding delegate. Not used except for 
        /// troubleshooting.
        /// </summary>
        public string BindingSource { get; set; }

        public Func<DisplayContext, IHtmlString> Binding { get; set; }

        public IEnumerable<Action<ShapeCreatingContext>> Creating {get;set;}

        public IEnumerable<Action<ShapeCreatedContext>> Created {get;set;}
    }
}