namespace Orchard.WebApi.Filters {
    /// <summary>
    /// Any implementation of <see cref="IApiFilterProvider"/> will be injected as a WebAPI filter.
    /// </summary>
    public interface IApiFilterProvider : IDependency {
         
    }
}