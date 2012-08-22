using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Orchard.Projections.Settings.Serialization {
    public class XmlSettingsSerializer : ISettingsSerializer {
        private const string Root = "SObject";

        public void Serialize(TextWriter tw, SObject o) {
            using (var writer = new XmlTextWriter(tw)) {
                writer.WriteStartDocument();
                writer.WriteStartElement(Root);
                WriteGrappe(writer, o);

                writer.WriteEndDocument();
            }
        }

        public SObject Deserialize(TextReader tr) {
            var reader = new XmlTextReader(tr);
            var result = new SObject();

            // ignore root element
            while (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == Root) {
                reader.Read();
            }

            while (reader.MoveToContent() == XmlNodeType.Element) {
                ReadElement(reader, result);
            }

            return result;
        }

        private void ReadElement(XmlReader reader, SObject parent) {
            var name = XmlConvert.DecodeName(reader.LocalName);
            var type = reader["type"];

            // is it a value node ? i.e. type=""
            if (type != null) {
                if (type == "Array") {
                    // go to first item
                    parent.SetMember(name, ReadArray(reader));
                    reader.Read();
                }
                else {
                    var typeCode = (TypeCode)Enum.Parse(typeof(TypeCode), type);
                    var value = SConvert.XmlDecode(typeCode, reader.ReadElementString());
                    parent.SetMember(name, value);
                }
            }
            else {
                var grappe = new SObject();
                reader.Read();
                parent.SetMember(name, grappe);
                while (reader.MoveToContent() == XmlNodeType.Element) {
                    ReadElement(reader, grappe);
                }

                reader.Read();
            }
        }

        public SArray ReadArray(XmlReader reader) {
            var list = new List<object>();
            reader.Read();
            while (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "Item") {
                dynamic o = new SObject();
                ReadElement(reader, o);
                list.Add(o.Item);
            }

            return new SArray(list.ToArray());
        }

        private void WriteGrappe(XmlWriter writer, SObject grappe) {
            foreach (var pair in grappe._properties) {
                WriteAny(writer, pair.Key, pair.Value);
            }
        }

        private void WriteAny(XmlWriter writer, string name, object value) {
            if (value is SObject) {
                writer.WriteStartElement(XmlConvert.EncodeLocalName(name));
                WriteGrappe(writer, (SObject)value);
                writer.WriteEndElement();
            }
            if (value is SArray) {
                writer.WriteStartElement(XmlConvert.EncodeLocalName(name));
                writer.WriteAttributeString("type", "Array");
                foreach (var v in ((SArray)value).Values) {
                    WriteAny(writer, "Item", v);
                }
                writer.WriteEndElement();
            }
            else if (value is SValue) {
                var sValue = (SValue)value;
                writer.WriteStartElement(XmlConvert.EncodeLocalName(name));
                writer.WriteAttributeString("type", Type.GetTypeCode(sValue.Value.GetType()).ToString());
                writer.WriteString(SConvert.XmlEncode(sValue.Value));
                writer.WriteEndElement();
            }
        }
    }
}