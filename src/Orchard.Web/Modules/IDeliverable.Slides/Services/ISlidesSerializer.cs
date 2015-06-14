using System.Collections.Generic;
using IDeliverable.Slides.Models;
using Orchard;

namespace IDeliverable.Slides.Services
{
    public interface ISlidesSerializer : IDependency
    {
        string Serialize(IEnumerable<Slide> value);
        IEnumerable<Slide> Deserialize(string value);
    }
}