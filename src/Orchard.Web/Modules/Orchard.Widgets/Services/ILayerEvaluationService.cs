namespace Orchard.Widgets.Services
{
    public interface ILayerEvaluationService : IDependency {
        int[] GetActiveLayerIds();
    }
}