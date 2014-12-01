using System.Collections.Generic;

namespace Orchard.Layouts.Framework.Elements {
    public interface IContainer : IElement {
        IList<IElement> Elements { get; set; }
    }
}