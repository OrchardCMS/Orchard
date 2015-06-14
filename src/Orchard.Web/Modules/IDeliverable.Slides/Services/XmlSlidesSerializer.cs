using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using IDeliverable.Slides.Models;
using Orchard;

namespace IDeliverable.Slides.Services
{
    public class XmlSlidesSerializer : Component, ISlidesSerializer
    {
        public string Serialize(IEnumerable<Slide> value)
        {
            var element = new XElement("Slides", value.Select(x => new XElement("Slide", x.LayoutData)));
            return element.ToString(SaveOptions.DisableFormatting);
        }

        public IEnumerable<Slide> Deserialize(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return Enumerable.Empty<Slide>();

            var element = XElement.Parse(value);
            return element.Elements().Select(x => new Slide
            {
                LayoutData = x.Value
            });
        }
    }
}