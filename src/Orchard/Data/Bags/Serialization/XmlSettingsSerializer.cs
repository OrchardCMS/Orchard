using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Orchard.Data.Bags.Serialization {
    public class XmlSettingsSerializer : IBagSerializer {
        private const string Root = "Bag";

        public void Serialize(TextWriter tw, Bag o) {
            using (var writer = new XmlTextWriter(tw)) {
                writer.WriteStartDocument();
                writer.WriteStartElement(Root);
                WriteGrappe(writer, o);

                writer.WriteEndDocument();
            }
        }

        public Bag Deserialize(TextReader tr) {
            var reader = new XmlTextReader(tr);
            var result = new Bag();

            // ignore root element
            while (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == Root) {
                reader.Read();
            }

            while (reader.MoveToContent() == XmlNodeType.Element) {
                ReadElement(reader, result);
            }

            return result;
        }

        private void ReadElement(XmlReader reader, Bag parent) {
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
                var grappe = new Bag();
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
                dynamic o = new Bag();
                ReadElement(reader, o);
                list.Add(o.Item);
            }

            return new SArray(list.ToArray());
        }

        private void WriteGrappe(XmlWriter writer, Bag grappe) {
            foreach (var pair in grappe._properties) {
                WriteAny(writer, pair.Key, pair.Value);
            }
        }

        private void WriteAny(XmlWriter writer, string name, object value) {
            if (value is Bag) {
                writer.WriteStartElement(XmlConvert.EncodeLocalName(name));
                WriteGrappe(writer, (Bag)value);
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