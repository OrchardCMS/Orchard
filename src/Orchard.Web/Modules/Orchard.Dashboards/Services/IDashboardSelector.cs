namespace Orchard.Dashboards.Services {
    public interface IDashboardSelector : IDependency {
        DashboardSelectorResult GetDashboard();
    }
}