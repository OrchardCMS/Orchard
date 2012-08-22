using System.Web.Mvc;
using Orchard.Events;

namespace Orchard.Forms.Services {
    public interface IFormEventHandler : IEventHandler {
        void Building(BuildingContext context);
        void Built(BuildingContext context);
        void Validating(ValidatingContext context);
        void Validated(ValidatingContext context);
    }

    public class FormHandler : IFormEventHandler {

        public virtual void Building(BuildingContext context) {}

        public virtual void Built(BuildingContext context) {}

        public virtual void Validating(ValidatingContext context) {}

        public virtual void Validated(ValidatingContext context) {}
    }

    public class BuildingContext {
        public dynamic Shape { get; set; }
    }

    public class ValidatingContext {
        public string FormName { get; set; }
        public IValueProvider ValueProvider { get; set; }
        public ModelStateDictionary ModelState { get; set; }
    }
}