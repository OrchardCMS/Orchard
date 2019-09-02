using Orchard.Forms.Services;
using Orchard.Localization.Services;

namespace Orchard.DynamicForms.Services {
    public interface IFormElementServices : IDependency
    {
        IFormManager FormManager { get; }
        ICultureManager CultureManager { get; }
    }
}