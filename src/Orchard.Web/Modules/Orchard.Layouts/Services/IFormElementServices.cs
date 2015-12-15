using Orchard.Forms.Services;

namespace Orchard.Layouts.Services {
    public interface IFormsBasedElementServices : IDependency
    {
        IFormManager FormManager { get; }
        ICultureAccessor CultureAccessor { get; }
    }
}