using Orchard.Layouts.Framework.Elements;

namespace IDeliverable.Slides.Models
{
    public class SlideshowProfile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SelectedEngine { get; set; }
        public ElementDataDictionary EngineStates { get; set; }
    }
}