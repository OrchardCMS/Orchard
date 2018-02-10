namespace Orchard.Dashboards.Services {
    public interface IDashboardService : IDependency {
        DashboardDescriptor GetDashboardDescriptor();
        dynamic GetDashboardShape();
    }
}