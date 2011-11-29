using System.Xml.Linq;
using NUnit.Framework;
using Orchard.Core.XmlRpc.Models;
using Orchard.Core.XmlRpc.Services;

namespace Orchard.Tests.Modules.XmlRpc.Services {
    [TestFixture]
    public class XmlRpcWriterTests {
        [Test]
        public void MethodResponseWriterShouldSendParametersWithValues() {
            var mapper = new XmlRpcWriter();

            var response = new XRpcMethodResponse();
            response.Params.Add(new XRpcData<int> { Value = 42 });
            var element = mapper.MapMethodResponse(response);

            Assert.That(NoSpace(element.ToString()), Is.EqualTo("<methodResponse><params><param><value><int>42</int></value></param></params></methodResponse>"));
        }

        [Test]
        public void ArrayAndStructShouldWorkAsExpected() {
            var mapper = new XmlRpcWriter();

            var arr = new XRpcArray();
            var structParam = XRpcData.For(new XRpcStruct());

            arr.Data.Add(structParam);
            arr.Data.Add(XRpcData.For(19));

            structParam.Value.Members.Add("Hello", XRpcData.For("world"));
            
            var element = mapper.MapArray(arr);

            Assert.That(NoSpace(element.ToString()), Is.EqualTo(NoSpace(@"
<array><data>
<value><struct>
<member><name>Hello</name><value><string>world</string></value></member>
</struct></value>
<value><int>19</int></value>
</data></array>
")));
        }

        [Test]
        public void FaultShouldBeCorrectlyFormatted() {
            var mapper = new XmlRpcWriter();
            var response = new XRpcMethodResponse {
                Fault = new XRpcFault(10, "foo")
            };

            var element = mapper.MapMethodResponse(response);

            Assert.That(NoSpace(element.ToString()), Is.EqualTo(NoSpace(@"
<methodResponse><fault>
<value><struct>
<member><name>faultCode</name><value><int>10</int></value></member>
<member><name>faultString</name><value><string>foo</string></value></member>
</struct></value>
</fault></methodResponse>
")));
        }

        private static string NoSpace(string text) {
            return text.Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", "");
        }
    }
}