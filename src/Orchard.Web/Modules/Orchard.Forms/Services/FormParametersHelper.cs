using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.Data.Bags;

namespace Orchard.Forms.Services {
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
            var result = Bag.New();

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

        public static dynamic FromJsonString(string state) {
            if (string.IsNullOrWhiteSpace(state)) {
                return null;
            }

            return JsonConvert.DeserializeObject(state);
        }

        public static string ToJsonString(FormCollection formCollection) {
            var o = new JObject();

            foreach (var key in formCollection.AllKeys) {
                o.Add(new JProperty(key, formCollection.Get(key)));
            }

            return JsonConvert.SerializeObject(o);
        }

        public static string ToJsonString(IDictionary<string, object> dictionary) {
            var o = new JObject();

            foreach (var entry in dictionary) {
                o.Add(new JProperty(entry.Key, entry.Value));
            }

            return JsonConvert.SerializeObject(o);
        }
    }
}