using System.Web.Mvc;

namespace Orchard.DynamicForms.Services {
    public interface IController {
        TempDataDictionary TempData { get; }
        ModelStateDictionary ModelState { get; }
    }
}