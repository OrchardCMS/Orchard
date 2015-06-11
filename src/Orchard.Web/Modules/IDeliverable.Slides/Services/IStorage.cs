namespace IDeliverable.Slides.Services
{
    public interface IStorage
    {
        void Store<T>(string key, T value);
        T Retrieve<T>(string key);
    }
}