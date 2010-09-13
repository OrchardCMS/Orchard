using System.Web;
using Orchard.Localization;

namespace Orchard.Mvc {
    /// <summary>
    /// This interface ensures all base view pages implement the 
    /// same set of additional members
    /// </summary>
    public interface IOrchardViewPage {
        Localizer T { get; }
        dynamic Display { get; }
        IHtmlString DisplayChildren(object shape);
        WorkContext WorkContext { get; }
    }
}
