using Orchard.ContentManagement;

namespace IDeliverable.Slides.Services
{
    public class ContentPartStorage : IStorage
    {
        private readonly ContentPart _part;
        public ContentPartStorage(ContentPart part)
        {
            _part = part;
        }

        public void Store<T>(string key, T value)
        {
            _part.StoreVersioned(key, value);
        }

        public T Retrieve<T>(string key)
        {
            return _part.RetrieveVersioned<T>(key);
        }
    }
}