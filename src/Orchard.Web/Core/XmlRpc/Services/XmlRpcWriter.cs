using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.Core.XmlRpc.Models;

namespace Orchard.Core.XmlRpc.Services {
    public class XmlRpcWriter :
        IMapper<XRpcMethodResponse, XElement>,
        IMapper<XRpcStruct, XElement>,
        IMapper<XRpcArray, XElement>,
        IMapper<XRpcData, XElement> {

        public XmlRpcWriter() {
            _dispatch = new Dictionary<Type, Func<XRpcData, XElement>> 
                        {
                            {typeof(int), p=>new XElement("int", (int)p.Value)},
                            {typeof(bool), p=>new XElement("boolean", (bool)p.Value?"1":"0")},
                            {typeof(string), p=>new XElement("string", p.Value)},
                            {typeof(double), p=>new XElement("double", (double)p.Value)},
                            {typeof(DateTime), p=>new XElement("dateTime.iso8601", ((DateTime)p.Value).ToString("yyyyMMddTHH:mm:ssZ"))},
                            {typeof(DateTime?), p=>new XElement("dateTime.iso8601", ((DateTime?)p.Value).Value.ToString("yyyyMMddTHH:mm:ssZ"))},
                            {typeof(byte[]), p=>new XElement("base64", Convert.ToBase64String((byte[])p.Value))},
                            {typeof(XRpcStruct), p=>Map((XRpcStruct)p.Value)},
                            {typeof(XRpcArray), p=>Map((XRpcArray)p.Value)},
                        };
        }

        private readonly IDictionary<Type, Func<XRpcData, XElement>> _dispatch;

        XElement IMapper<XRpcMethodResponse, XElement>.Map(XRpcMethodResponse source) {
            return new XElement(
                "methodResponse",
                new XElement(
                    "params",
                    source.Params.Select(
                        p => new XElement("param", MapValue(p)))));
        }

        XElement IMapper<XRpcData, XElement>.Map(XRpcData source) {
            return new XElement("param", MapValue(source));
        }

        XElement IMapper<XRpcStruct, XElement>.Map(XRpcStruct source) {
            return new XElement(
                "struct",
                source.Members.Select(
                    kv => new XElement(
                              "member",
                              new XElement("name", kv.Key),
                              MapValue(kv.Value))));
        }

        XElement IMapper<XRpcArray, XElement>.Map(XRpcArray source) {
            return new XElement(
                "array",
                new XElement(
                    "data",
                    source.Data.Select(d => MapValue(d))));
        }

        XElement Map<T>(T t) {
            return ((IMapper<T, XElement>)this).Map(t);
        }

        private XElement MapValue(XRpcData data) {
            return new XElement("value", _dispatch[data.Type](data));
        }

        }
}