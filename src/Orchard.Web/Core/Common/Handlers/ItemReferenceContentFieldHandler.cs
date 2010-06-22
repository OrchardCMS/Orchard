using System.Collections.Generic;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Common.Handlers {
    public class ItemReferenceContentFieldHandler : ContentFieldHandler {
        public ItemReferenceContentFieldHandler(IEnumerable<IContentFieldDriver> contentFieldDrivers) : base(contentFieldDrivers) { }
    }
}