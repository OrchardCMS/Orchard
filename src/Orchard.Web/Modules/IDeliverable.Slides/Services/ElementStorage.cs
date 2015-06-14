using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;

namespace IDeliverable.Slides.Services
{
    public class ElementStorage : IStorage
    {
        private readonly Element _element;

        public ElementStorage(Element element)
        {
            _element = element;
        }

        public void Store<T>(string key, T value)
        {
            _element.Store(key, value);
        }

        public T Retrieve<T>(string key)
        {
            return _element.Retrieve<T>(key);
        }
    }
}