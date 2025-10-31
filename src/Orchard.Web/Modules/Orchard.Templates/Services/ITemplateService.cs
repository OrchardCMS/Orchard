using Orchard.DisplayManagement.Implementation;

namespace Orchard.Templates.Services
{
    public interface ITemplateService : IDependency
    {
        string Execute<TModel>(string template, string name, string processorName, TModel model = default(TModel));
        string Execute<TModel>(string template, string name, string processorName, DisplayContext context, TModel model = default(TModel));
    }
}