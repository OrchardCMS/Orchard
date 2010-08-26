using System.Web.Mvc;

namespace Orchard.DisplayManagement {
    public interface IDisplayHelperFactory : IDependency {
        DisplayHelper CreateDisplayHelper(ViewContext viewContext, IViewDataContainer viewDataContainer);
    }
}