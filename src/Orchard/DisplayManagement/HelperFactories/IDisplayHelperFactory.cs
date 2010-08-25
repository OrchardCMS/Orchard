using System.Web.Mvc;

namespace Orchard.DisplayManagement {
    public interface IDisplayHelperFactory : ISingletonDependency {
        DisplayHelper CreateDisplayHelper(ViewContext viewContext, IViewDataContainer viewDataContainer);
    }
}