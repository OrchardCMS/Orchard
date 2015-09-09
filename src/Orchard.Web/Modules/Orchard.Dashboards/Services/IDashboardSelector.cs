namespace Orchard.Dashboards.Services {
    public interface IDashboardSelector : IDependency {
        DashboardDescriptor GetDashboardDescriptor();
    }
}