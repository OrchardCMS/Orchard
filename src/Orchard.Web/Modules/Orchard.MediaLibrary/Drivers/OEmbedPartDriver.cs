using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.MediaLibrary.Models;
using System.Xml;
using System.Xml.Linq;
using Orchard.ContentManagement.Handlers;
using System.Collections;

namespace Orchard.MediaLibrary.Drivers {
    public class OEmbedPartDriver : ContentPartDriver<OEmbedPart> {
        protected override DriverResult Display(OEmbedPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_OEmbed_Metadata", () => shapeHelper.Parts_OEmbed_Metadata()),
                ContentShape("Parts_OEmbed_Summary", () => shapeHelper.Parts_OEmbed_Summary()),
                ContentShape("Parts_OEmbed_SummaryAdmin", () => shapeHelper.Parts_OEmbed_SummaryAdmin()),
                ContentShape("Parts_OEmbed", () => shapeHelper.Parts_OEmbed())
            );
        }

        protected override void Exporting(OEmbedPart part, ContentManagement.Handlers.ExportContentContext context) {
            var partName = XmlConvert.EncodeName(typeof(OEmbedPart).Name);

            var infosetPart = part.As<InfosetPart>();
            if (infosetPart != null) {
                // OEmbedPart is not versionable thats why using Infoset.Element instead of VersionInfoset.Element
                var element = infosetPart.Infoset.Element;

                var partElement = element.Element(partName);
                if (partElement == null)
                    return;

                context.Element(partName).Add(partElement.Elements());
            }
        }

        protected override void Importing(OEmbedPart part, ContentManagement.Handlers.ImportContentContext context) {
            var partName = XmlConvert.EncodeName(typeof(OEmbedPart).Name);

            // Don't do anything if the tag is not specified.
            var xmlElement = context.Data.Element(partName);
            if (xmlElement == null)
                return;

            var infosetPart = part.As<InfosetPart>();
            if (infosetPart != null) {
                // OEmbedPart is not versionable thats why using Infoset.Element instead of VersionInfoset.Element
                var element = infosetPart.Infoset.Element;

                var partElement = element.Element(partName);
                if (partElement != null)
                    partElement.Remove();

                partElement = new XElement(partName);
                element.Add(partElement);
                partElement.Add(xmlElement.Elements());
            }
        }

        protected override void Cloning(OEmbedPart originalPart, OEmbedPart clonePart, CloneContentContext context) {
            var partName = XmlConvert.EncodeName(typeof(OEmbedPart).Name);

            var infosetOriginalPart = originalPart.As<InfosetPart>();
            var infosetClonePart = clonePart.As<InfosetPart>();
            if (infosetOriginalPart != null && infosetClonePart != null) {
                // OEmbedPart is not versionable thats why using Infoset.Element instead of VersionInfoset.Element
                var originalElement = infosetOriginalPart.Infoset.Element;
                var partOriginalElement = originalElement.Element(partName);
                var cloneElement = infosetClonePart.Infoset.Element;
                var partCloneElement = cloneElement.Element(partName);

                if (partCloneElement != null)
                    partCloneElement.Remove();

                partCloneElement = new XElement(partName);
                cloneElement.Add(partCloneElement);
                partCloneElement.Add(partOriginalElement.Elements());
            }
        }
    }
}