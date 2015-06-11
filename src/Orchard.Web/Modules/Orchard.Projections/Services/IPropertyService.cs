namespace Orchard.Projections.Services {
    public interface IPropertyService : IDependency {
        void MoveUp(int propertyId);
        void MoveDown(int propertyId);
    }
}
