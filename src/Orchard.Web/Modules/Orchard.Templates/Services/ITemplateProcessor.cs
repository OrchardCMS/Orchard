using Orchard.DisplayManagement.Implementation;

namespace Orchard.Templates.Services {
    public interface ITemplateProcessor : IDependency {
        string Type { get; }
        string Process(string template, string name, DisplayContext context = null, dynamic model = null);
        void Verify(string template);
    }
}