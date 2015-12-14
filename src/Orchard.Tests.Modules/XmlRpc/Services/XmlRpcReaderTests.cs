using System;
using System.Xml.Linq;
using NUnit.Framework;
using Orchard.Core.XmlRpc.Services;

namespace Orchard.Tests.Modules.XmlRpc.Services {
    [TestFixture]
    public class XmlRpcReaderTests {
        private IXmlRpcReader _xmlRpcReader;

        [SetUp]
        public void Init() {
            _xmlRpcReader = new XmlRpcReader();
        }

        [Test]
        public void MethodCallShouldMapName() {
            var source = XElement.Parse(@"
<methodCall>
    <methodName>hello world</methodName>
</methodCall>");

            var methodCall = _xmlRpcReader.MapToMethodCall(source);
            Assert.That(methodCall, Is.Not.Null);
            Assert.That(methodCall.MethodName, Is.EqualTo("hello world"));
        }

        [Test]
        public void CallWithParametersShouldMapValuesAccordingToSpec() {
            var source = XElement.Parse(@"
<methodCall>
    <methodName>hello world</methodName>
    <params>
        <param><value><i4>-12</i4></value></param>
        <param><value><int>42</int></value></param>
        <param><value><boolean>1</boolean></value></param>
        <param><value><boolean>0</boolean></value></param>
        <param><value><string>hello world</string></value></param>
        <param><value><double>-12.214</double></value></param>
        <param><value><dateTime.iso8601>1998-07-17T14:08:55</dateTime.iso8601></value></param>
        <param><value><base64>eW91IGNhbid0IHJlYWQgdGhpcyE=</base64></value></param>
    </params>
</methodCall>");

            var methodCall = _xmlRpcReader.MapToMethodCall(source);
            Assert.That(methodCall, Is.Not.Null);
            Assert.That(methodCall.Params, Has.Count.EqualTo(8));
            Assert.That(methodCall.Params[0].Value, Is.EqualTo(-12));
            Assert.That(methodCall.Params[1].Value, Is.EqualTo(42));
            Assert.That(methodCall.Params[2].Value, Is.EqualTo(true));
            Assert.That(methodCall.Params[3].Value, Is.EqualTo(false));
            Assert.That(methodCall.Params[4].Value, Is.EqualTo("hello world"));
            Assert.That(methodCall.Params[5].Value, Is.EqualTo(-12.214));
            Assert.That(methodCall.Params[6].Value, Is.EqualTo(new DateTime(1998, 7, 17, 14, 8, 55)));
            Assert.That(methodCall.Params[7].Value, Is.EqualTo(Convert.FromBase64String("eW91IGNhbid0IHJlYWQgdGhpcyE=")));
        }

        [Test]
        public void StructShouldMapAllMembersByNameWithCorrectType() {
            var source = XElement.Parse(@"
<struct>
    <member><name>one</name><value><i4>-12</i4></value></member>
    <member><name>two</name><value><int>42</int></value></member>
    <member><name>three</name><value><boolean>1</boolean></value></member>
    <member><name>four</name><value><boolean>0</boolean></value></member>
    <member><name>five</name><value><string>hello world</string></value></member>
    <member><name>six</name><value><double>-12.214</double></value></member>
    <member><name>seven</name><value><dateTime.iso8601>1998-07-17T14:08:55</dateTime.iso8601></value></member>
    <member><name>eight</name><value><base64>eW91IGNhbid0IHJlYWQgdGhpcyE=</base64></value></member>
</struct>");

            var xmlStruct = _xmlRpcReader.MapToStruct(source);
            Assert.That(xmlStruct["one"], Is.EqualTo(-12));
            Assert.That(xmlStruct["two"], Is.EqualTo(42));
            Assert.That(xmlStruct["three"], Is.EqualTo(true));
            Assert.That(xmlStruct["four"], Is.EqualTo(false));
            Assert.That(xmlStruct["five"], Is.EqualTo("hello world"));
            Assert.That(xmlStruct["six"], Is.EqualTo(-12.214));
            Assert.That(xmlStruct["seven"], Is.EqualTo(new DateTime(1998, 7, 17, 14, 8, 55)));
            Assert.That(xmlStruct["eight"], Is.EqualTo(Convert.FromBase64String("eW91IGNhbid0IHJlYWQgdGhpcyE=")));

        }

        [Test]
        public void StructShouldMapDefaultDateTimeWithBadFormat() {
            var source = XElement.Parse(@"
<struct>
    <member><name>seven</name><value><dateTime.iso8601>FOO</dateTime.iso8601></value></member>
</struct>");

            var xmlStruct = _xmlRpcReader.MapToStruct(source);
            Assert.That(xmlStruct["seven"], Is.GreaterThan(DateTime.Now.AddSeconds(-1)));
            Assert.That(xmlStruct["seven"], Is.LessThan(DateTime.Now.AddSeconds(1)));
        }

        [Test]
        public void ArrayShouldBringDataItemsWithCorrectType() {
            var source = XElement.Parse(@"
<array>
   <data>
      <value><i4>12</i4></value>
      <value><string>Egypt</string></value>
      <value><boolean>0</boolean></value>
      <value><i4>-31</i4></value>
      </data>
   </array>
");

            var xmlArray = _xmlRpcReader.MapToArray(source);
            Assert.That(xmlArray.Data, Has.Count.EqualTo(4));
            Assert.That(xmlArray[0], Is.EqualTo(12));
            Assert.That(xmlArray[1], Is.EqualTo("Egypt"));
            Assert.That(xmlArray[2], Is.EqualTo(false));
            Assert.That(xmlArray[3], Is.EqualTo(-31));
        }
    }
}