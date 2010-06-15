using System.Collections.Generic;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Common.Handlers {
    public class TextContentFieldHandler : ContentFieldHandler {
        public TextContentFieldHandler(IEnumerable<IContentFieldDriver> contentFieldDrivers) : base(contentFieldDrivers) {}
    }
}