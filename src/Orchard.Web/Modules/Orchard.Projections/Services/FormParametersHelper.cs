using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Orchard.Projections.Settings;

namespace Orchard.Projections.Services {
    public static class FormParametersHelper {
        public static string ToString(IDictionary<string, string> parameters) {
            var doc = new XDocument();
            doc.Add(new XElement("Form"));
            var root = doc.Root;

            if (root == null) {
                return String.Empty;
            }

            foreach (var entry in parameters) {
                root.Add(new XElement(XmlConvert.EncodeLocalName(entry.Key), entry.Value));
            }

            return doc.ToString(SaveOptions.DisableFormatting);
        }

        public static IDictionary<string, string> FromString(string parameters) {
            var result = new Dictionary<string, string>();

            if (String.IsNullOrEmpty(parameters)) {
                return result;
            }

            var doc = XDocument.Parse(parameters);
            if (doc.Root == null) {
                return result;
            }

            foreach (var element in doc.Root.Elements()) {
                result.Add(element.Name.LocalName, element.Value);
            }

            return result;
        }


        public static dynamic ToDynamic(string parameters) {
            var result = SObject.New();

            if (String.IsNullOrEmpty(parameters)) {
                return result;
            }

            var doc = XDocument.Parse(parameters);
            if (doc.Root == null) {
                return result;
            }

            foreach (var element in doc.Root.Elements()) {
                result[element.Name.LocalName] = element.Value;
            }

            return result;
        }
    }
}