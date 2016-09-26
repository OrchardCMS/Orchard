using Orchard.ContentManagement;
using Orchard.Layouts.Models;
using Orchard.Layouts.ViewModels;

namespace Orchard.Layouts.Services {
    public interface ILayoutEditorFactory : IDependency {
        LayoutEditor Create(LayoutPart layoutPart);
        LayoutEditor Create(string layoutData, string sessionKey, int? templateId = null, IContent content = null);
    }
}