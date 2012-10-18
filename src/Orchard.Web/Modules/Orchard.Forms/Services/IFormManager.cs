using System.Web.Mvc;

namespace Orchard.Forms.Services {
    public interface IFormManager : IDependency {
        dynamic Build(string formName, string prefix = "");
        dynamic Bind(dynamic formShape, IValueProvider state, string prefix = "");
        void Validate(ValidatingContext context);
    }
}