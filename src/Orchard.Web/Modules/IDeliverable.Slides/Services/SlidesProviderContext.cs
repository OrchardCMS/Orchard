using IDeliverable.Slides.Models;
using Orchard.ContentManagement;

namespace IDeliverable.Slides.Services
{
    public class SlidesProviderContext
    {
        public SlidesProviderContext(IContent content, ISlideShow slideShow, IStorage storage, string elementSessionKey = null)
        {
            Content = content;
            SlideShow = slideShow;
            Storage = storage;
            ElementSessionKey = elementSessionKey;
        }

        public IContent Content { get; private set; }
        public ISlideShow SlideShow { get; private set; }
        public IStorage Storage { get; set; }
        public string ElementSessionKey { get; private set; }
    }
}