using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Orchard.Core.XmlRpc.Models;

namespace Orchard.Core.XmlRpc.Services {
    public class XmlRpcReader :
        IMapper<XElement, XRpcMethodCall>,
        IMapper<XElement, XRpcData>,
        IMapper<XElement, XRpcStruct>,
        IMapper<XElement, XRpcArray> {

        private readonly IDictionary<string, Func<XElement, XRpcData>> _dispatch;

        public XmlRpcReader() {
            _dispatch = new Dictionary<string, Func<XElement, XRpcData>>
                        {
                            {"i4",x=>new XRpcData<int> { Value = (int)x }},
                            {"int", x=>new XRpcData<int> { Value = (int)x }}, 
                            {"boolean", x=>new XRpcData<bool> { Value = ((string)x=="1") }}, 
                            {"string", x=>new XRpcData<string> { Value = (string)x }}, 
                            {"double", x=>new XRpcData<double> { Value = (double)x }}, 
                            {"dateTime.iso8601", x=> {
                                                     DateTime parsedDateTime;
                                                     // try parsing a "normal" datetime string then try what live writer gives us
                                                     if(!DateTime.TryParse(x.Value, out parsedDateTime)
                                                         && !DateTime.TryParseExact(x.Value, "yyyyMMddTHH:mm:ss", DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out parsedDateTime)) {
                                                         parsedDateTime = DateTime.Now;
                                                     }
                                                     return new XRpcData<DateTime> {Value = parsedDateTime};
                                                 }},
                            {"base64", x=>new XRpcData<byte[]> { Value = Convert.FromBase64String((string)x) }}, 
                            {"struct", x=>XRpcData.For(Map<XRpcStruct>(x))} , 
                            {"array", x=>XRpcData.For(Map<XRpcArray>(x))} , 
                        };
        }

        T2 Map<T2>(XElement t1) {
            return ((IMapper<XElement, T2>)this).Map(t1);
        }

        XRpcData MapValue(XContainer t1) {
            var element = t1.Elements().SingleOrDefault();

            Func<XElement, XRpcData> dispatch;
            if (_dispatch.TryGetValue(element.Name.LocalName, out dispatch) == false)
                throw new ApplicationException("Unknown XmlRpc value type " + element.Name.LocalName);

            return dispatch(element);
        }

        XRpcMethodCall IMapper<XElement, XRpcMethodCall>.Map(XElement source) {
            return new XRpcMethodCall {
                MethodName = (string)source.Element("methodName"),
                Params = source.Elements("params").Elements("param").Select(Map<XRpcData>).ToList()
            };
        }


        XRpcData IMapper<XElement, XRpcData>.Map(XElement source) {
            var value = source.Element("value");
            if (value == null)
                return new XRpcData();

            var element = value.Elements().SingleOrDefault();

            Func<XElement, XRpcData> dispatch;
            if (_dispatch.TryGetValue(element.Name.LocalName, out dispatch) == false)
                throw new ApplicationException("Unknown XmlRpc value type " + element.Name.LocalName);

            return dispatch(element);
        }

        XRpcStruct IMapper<XElement, XRpcStruct>.Map(XElement source) {
            var result = new XRpcStruct();
            foreach (var member in source.Elements("member")) {
                result.Members.Add(
                    (string)member.Element("name"),
                    MapValue(member.Element("value")));
            }
            return result;
        }


        XRpcArray IMapper<XElement, XRpcArray>.Map(XElement source) {
            var result = new XRpcArray();
            foreach (var value in source.Elements("data").Elements("value")) {
                result.Data.Add(MapValue(value));
            }
            return result;
        }

    }
}