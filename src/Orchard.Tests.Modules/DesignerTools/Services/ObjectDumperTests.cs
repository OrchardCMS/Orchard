using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.DesignerTools.Services;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.Tests.Modules.DesignerTools.Services
{
    public class TestObject
    {
        public int SomeInteger { get; set; }
        public bool SomeBoolean { get; set; }
        public string SomeString { get; set; }
        public TestObject ChildObject { get; set; }
    }

    public class TestIShape : IShape
    {
        public ShapeMetadata Metadata { get; set; }

        public int SomeInteger { get; set; }
        public bool SomeBoolean { get; set; }
        public string SomeString { get; set; }

        public TestIShape ChildObject { get; set; }
    }

    public class TestShape : Shape
    {
        public int SomeInteger { get; set; }
        public bool SomeBoolean { get; set; }
        public string SomeString { get; set; }

        public TestShape ChildObject { get; set; }
    }

    [TestFixture]
    public class ObjectDumperTests
    {
        private static void ComparareJsonObject(JObject expectedResult, string json) {
            var objectDumperJson = JToken.Parse("{" + json + "}");

            string message = String.Format("expected: {0} \r\nresult:{1}", expectedResult.ToString(Formatting.Indented), objectDumperJson.ToString(Formatting.Indented));

            Assert.IsTrue(JToken.DeepEquals(expectedResult, objectDumperJson), message);
        }

        [Test]
        public void DumpNull() {
            var objectDumper = new ObjectDumper(1);
            var xElement = objectDumper.Dump(null, "Model");

            var stringBuilder = new StringBuilder();
            ObjectDumper.ConvertToJSon(xElement, stringBuilder);
            var json = stringBuilder.ToString();

            var jObject = new JObject(
                new JProperty("name", "Model"), 
                new JProperty("value", "null"));

            ComparareJsonObject(jObject, json);
        }

        [Test]
        public void DumpValueTypeInteger() {
            var objectDumper = new ObjectDumper(1);
            var xElement = objectDumper.Dump(1337, "Model");

            var stringBuilder = new StringBuilder();
            ObjectDumper.ConvertToJSon(xElement, stringBuilder);
            var json = stringBuilder.ToString();

            var jObject = new JObject(
                new JProperty("name", "Model"), 
                new JProperty("value", "1337"));

            ComparareJsonObject(jObject, json);
        }

        [Test]
        public void DumpValueTypeBoolean() {
            var objectDumper = new ObjectDumper(1);
            var xElement = objectDumper.Dump(true, "Model");

            var stringBuilder = new StringBuilder();
            ObjectDumper.ConvertToJSon(xElement, stringBuilder);
            var json = stringBuilder.ToString();

            var jObject = new JObject(
                new JProperty("name", "Model"), 
                new JProperty("value", "True"));

            ComparareJsonObject(jObject, json);
        }

        [Test]
        public void DumpString() {
            var objectDumper = new ObjectDumper(1);
            var xElement = objectDumper.Dump("Never gonna give you up", "Model");

            var stringBuilder = new StringBuilder();
            ObjectDumper.ConvertToJSon(xElement, stringBuilder);
            var json = stringBuilder.ToString();

            var jObject = new JObject(
                new JProperty("name", "Model"),
                new JProperty("value", "&quot;Never gonna give you up&quot;"));

            ComparareJsonObject(jObject, json);
        }

        [Test]
        public void DumpEnumerable() {
            var enumerable = new[] { 1, 2, 3 }.AsEnumerable();

            var objectDumper = new ObjectDumper(1);
            var xElement = objectDumper.Dump(enumerable, "Model");

            Assert.Throws(typeof(NullReferenceException), () => {
                var stringBuilder = new StringBuilder();
                ObjectDumper.ConvertToJSon(xElement, stringBuilder);
                var json = stringBuilder.ToString();
            });
        }

        [Test]
        public void DumpEnumerable_DepthTwo() {
            var enumerable = new[] { 1, 2, 3 }.AsEnumerable();

            var objectDumper = new ObjectDumper(2);
            var xElement = objectDumper.Dump(enumerable, "Model");

            var stringBuilder = new StringBuilder();
            ObjectDumper.ConvertToJSon(xElement, stringBuilder);
            var json = stringBuilder.ToString();

            var jObject = new JObject(
                new JProperty("name", "Model"), 
                new JProperty("value", "Int32[]"), 
                new JProperty("children", new JArray(
                    new JObject(
                        new JProperty("name", "[0]"), 
                        new JProperty("value", "1")),
                    new JObject(
                        new JProperty("name", "[1]"), 
                        new JProperty("value", "2")),
                    new JObject(
                        new JProperty("name", "[2]"), 
                        new JProperty("value", "3"))
                    )));

            ComparareJsonObject(jObject, json);
        }

        [Test]
        public void DumpDictionary() {
            var dictionary = new Dictionary<string, int> { { "One", 1 }, { "Two", 2 }, {"Three", 3} };

            var objectDumper = new ObjectDumper(1);
            var xElement = objectDumper.Dump(dictionary, "Model");

            Assert.Throws(typeof(NullReferenceException), () => {
                var stringBuilder = new StringBuilder();
                ObjectDumper.ConvertToJSon(xElement, stringBuilder);
                var json = stringBuilder.ToString();
            });
        }

        [Test]
        public void DumpDictionary_DepthTwo() {
            var dictionary = new Dictionary<string, int> { { "One", 1 }, { "Two", 2 }, { "Three", 3 } };

            var objectDumper = new ObjectDumper(2);
            var xElement = objectDumper.Dump(dictionary, "Model");

            var stringBuilder = new StringBuilder();
            ObjectDumper.ConvertToJSon(xElement, stringBuilder);
            var json = stringBuilder.ToString();

            var jObject = new JObject(
                new JProperty("name", "Model"),
                new JProperty("value", "Dictionary&lt;String, Int32&gt;"),
                new JProperty("children", new JArray(
                    new JObject(
                        new JProperty("name", "[&quot;One&quot;]"),
                        new JProperty("value", "1")),
                    new JObject(
                        new JProperty("name", "[&quot;Two&quot;]"),
                        new JProperty("value", "2")),
                    new JObject(
                        new JProperty("name", "[&quot;Three&quot;]"),
                        new JProperty("value", "3"))
                    )));

            ComparareJsonObject(jObject, json);
        }

        [Test]
        public void DumpContentItem_DepthTwo() {
            var contentItem = new ContentItem { ContentType = "TestContentType" };
            var testingPart = new ContentPart { TypePartDefinition = new ContentTypePartDefinition(new ContentPartDefinition("TestingPart"), new SettingsDictionary()) };
            contentItem.Weld(testingPart);

            var objectDumper = new ObjectDumper(2);
            var xElement = objectDumper.Dump(contentItem, "Model");

            var stringBuilder = new StringBuilder();
            ObjectDumper.ConvertToJSon(xElement, stringBuilder);
            var json = stringBuilder.ToString();

            var jObject = new JObject(
                new JProperty("name", "Model"),
                new JProperty("value", "ContentItem"),
                new JProperty("children", new JArray(
                    new JObject(
                        new JProperty("name", "Id"),
                        new JProperty("value", "0")),
                    new JObject(
                        new JProperty("name", "Version"),
                        new JProperty("value", "0")),
                    new JObject(
                        new JProperty("name", "ContentType"),
                        new JProperty("value", "&quot;TestContentType&quot;"))
                    )));

            ComparareJsonObject(jObject, json);
        }

        [Test]
        public void DumpContentItem_DepthFour() {
            var contentItem = new ContentItem { ContentType = "TestContentType" };
            var testingPart = new ContentPart { TypePartDefinition = new ContentTypePartDefinition(new ContentPartDefinition("TestingPart"), new SettingsDictionary()) };
            contentItem.Weld(testingPart);

            var objectDumper = new ObjectDumper(4);
            var xElement = objectDumper.Dump(contentItem, "Model");

            var stringBuilder = new StringBuilder();
            ObjectDumper.ConvertToJSon(xElement, stringBuilder);
            var json = stringBuilder.ToString();

            var jObject = new JObject(
                new JProperty("name", "Model"),
                new JProperty("value", "ContentItem"),
                new JProperty("children", new JArray(
                    new JObject(
                        new JProperty("name", "Id"),
                        new JProperty("value", "0")),
                    new JObject(
                        new JProperty("name", "Version"),
                        new JProperty("value", "0")),
                    new JObject(
                        new JProperty("name", "ContentType"),
                        new JProperty("value", "&quot;TestContentType&quot;")),
                    new JObject(
                        new JProperty("name", "TestingPart"),
                        new JProperty("value", "ContentPart"),
                        new JProperty("children", new JArray(
                            new JObject(
                                new JProperty("name", "Id"),
                                new JProperty("value", "0")),
                            new JObject(
                                new JProperty("name", "TypeDefinition"),
                                new JProperty("value", "null")),
                            new JObject(
                                new JProperty("name", "TypePartDefinition"),
                                new JProperty("value", "ContentTypePartDefinition"),
                                new JProperty("children", new JArray(
                                    new JObject(
                                        new JProperty("name", "ContentTypeDefinition"),
                                        new JProperty("value", "null"))))),
                            new JObject(
                                new JProperty("name", "PartDefinition"),
                                new JProperty("value", "ContentPartDefinition"),
                                new JProperty("children", new JArray(
                                    new JObject(
                                        new JProperty("name", "Name"),
                                        new JProperty("value", "&quot;TestingPart&quot;"))))),
                            new JObject(
                                new JProperty("name", "Settings"),
                                new JProperty("value", "SettingsDictionary")),
                            new JObject(
                                new JProperty("name", "Fields"),
                                new JProperty("value", "List&lt;ContentField&gt;"))))))));

            ComparareJsonObject(jObject, json);
        }

        [Test]
        public void DumpContentItem_DepthSix() {
            var contentItem = new ContentItem { ContentType = "TestContentType" };
            var testingPart = new ContentPart { TypePartDefinition = new ContentTypePartDefinition(new ContentPartDefinition("TestingPart"), new SettingsDictionary()) };
            contentItem.Weld(testingPart);

            var objectDumper = new ObjectDumper(6);
            var xElement = objectDumper.Dump(contentItem, "Model");

            var stringBuilder = new StringBuilder();
            ObjectDumper.ConvertToJSon(xElement, stringBuilder);
            var json = stringBuilder.ToString();

            var jObject = new JObject(
                new JProperty("name", "Model"),
                new JProperty("value", "ContentItem"),
                new JProperty("children", new JArray(
                    new JObject(
                        new JProperty("name", "Id"),
                        new JProperty("value", "0")),
                    new JObject(
                        new JProperty("name", "Version"),
                        new JProperty("value", "0")),
                    new JObject(
                        new JProperty("name", "ContentType"),
                        new JProperty("value", "&quot;TestContentType&quot;")),
                    new JObject(
                        new JProperty("name", "TestingPart"),
                        new JProperty("value", "ContentPart"),
                        new JProperty("children", new JArray(
                            new JObject(
                                new JProperty("name", "Id"),
                                new JProperty("value", "0")),
                            new JObject(
                                new JProperty("name", "TypeDefinition"),
                                new JProperty("value", "null")),
                            new JObject(
                                new JProperty("name", "TypePartDefinition"),
                                new JProperty("value", "ContentTypePartDefinition"),
                                new JProperty("children", new JArray(
                                    new JObject(
                                        new JProperty("name", "PartDefinition"),
                                        new JProperty("value", "ContentPartDefinition"),
                                        new JProperty("children", new JArray(
                                                new JObject(
                                                    new JProperty("name", "Name"),
                                                    new JProperty("value", "&quot;TestingPart&quot;")),
                                                new JObject(
                                                    new JProperty("name", "Fields"),
                                                    new JProperty("value", "ContentPartFieldDefinition[]")),
                                                new JObject(
                                                    new JProperty("name", "Settings"),
                                                    new JProperty("value", "SettingsDictionary"))))),
                                    new JObject(
                                        new JProperty("name", "Settings"),
                                        new JProperty("value", "SettingsDictionary")),
                                    new JObject(
                                        new JProperty("name", "ContentTypeDefinition"),
                                        new JProperty("value", "null"))))),
                            new JObject(
                                new JProperty("name", "PartDefinition"),
                                new JProperty("value", "ContentPartDefinition"),
                                new JProperty("children", new JArray(
                                    new JObject(
                                        new JProperty("name", "Name"),
                                        new JProperty("value", "&quot;TestingPart&quot;")),
                                    new JObject(
                                        new JProperty("name", "Fields"),
                                        new JProperty("value", "ContentPartFieldDefinition[]")),
                                    new JObject(
                                        new JProperty("name", "Settings"),
                                        new JProperty("value", "SettingsDictionary"))))),
                            new JObject(
                                new JProperty("name", "Settings"),
                                new JProperty("value", "SettingsDictionary")),
                            new JObject(
                                new JProperty("name", "Fields"),
                                new JProperty("value", "List&lt;ContentField&gt;"))))))));

            ComparareJsonObject(jObject, json);
        }

        [Test]
        public void DumpObject_DepthOne() {
            var objectDumper = new ObjectDumper(1);
            var xElement = objectDumper.Dump(new TestObject
            {
                SomeInteger = 1337,
                SomeBoolean = true,
                SomeString = "Never gonna give you up"
            }, "Model");

            Assert.Throws(typeof (NullReferenceException), () => {
                var stringBuilder = new StringBuilder();
                ObjectDumper.ConvertToJSon(xElement, stringBuilder);
                var json = stringBuilder.ToString();
            });
        }

        [Test]
        public void DumpObject_DepthTwo() {
            var objectDumper = new ObjectDumper(2);
            var xElement = objectDumper.Dump(new TestObject
            {
                SomeInteger = 1337,
                SomeBoolean = true,
                SomeString = "Never gonna give you up"
            }, "Model");

            var stringBuilder = new StringBuilder();
            ObjectDumper.ConvertToJSon(xElement, stringBuilder);
            var json = stringBuilder.ToString();

            var jObject = new JObject(
                new JProperty("name", "Model"),
                new JProperty("value", "TestObject"),
                new JProperty("children", new JArray(
                    new JObject(
                        new JProperty("name", "SomeInteger"),
                        new JProperty("value", "1337")),
                    new JObject(
                        new JProperty("name", "SomeBoolean"),
                        new JProperty("value", "True")),
                    new JObject(
                        new JProperty("name", "SomeString"),
                        new JProperty("value", "&quot;Never gonna give you up&quot;")),
                    new JObject(
                        new JProperty("name", "ChildObject"),
                        new JProperty("value", "null")
                    )
                )));

            ComparareJsonObject(jObject, json);
        }

        [Test]
        public void DumpObjectAndChild_DepthTwo() {
            var objectDumper = new ObjectDumper(2);
            var xElement = objectDumper.Dump(new TestObject
            {
                SomeInteger = 1337,
                SomeBoolean = true,
                SomeString = "Never gonna give you up",
                ChildObject = new TestObject() {
                    SomeInteger = 58008,
                    SomeBoolean = false,
                    SomeString = "Never gonna let you down",                    
                }
            }, "Model");

            var stringBuilder = new StringBuilder();
            ObjectDumper.ConvertToJSon(xElement, stringBuilder);
            var json = stringBuilder.ToString();

            var jObject = new JObject(
                new JProperty("name", "Model"),
                new JProperty("value", "TestObject"),
                new JProperty("children", new JArray(
                    new JObject(
                        new JProperty("name", "SomeInteger"),
                        new JProperty("value", "1337")),
                    new JObject(
                        new JProperty("name", "SomeBoolean"),
                        new JProperty("value", "True")),
                    new JObject(
                        new JProperty("name", "SomeString"),
                        new JProperty("value", "&quot;Never gonna give you up&quot;"))
                    )));

            ComparareJsonObject(jObject, json);
        }

        [Test]
        public void DumpObjectAndChild_DepthThree() {
            var objectDumper = new ObjectDumper(3);
            var xElement = objectDumper.Dump(new TestObject
            {
                SomeInteger = 1337,
                SomeBoolean = true,
                SomeString = "Never gonna give you up",
                ChildObject = new TestObject() {
                    SomeInteger = 58008,
                    SomeBoolean = false,
                    SomeString = "Never gonna let you down",                    
                }
            }, "Model");

            var stringBuilder = new StringBuilder();
            ObjectDumper.ConvertToJSon(xElement, stringBuilder);
            var json = stringBuilder.ToString();

            var jObject = new JObject(
                new JProperty("name", "Model"),
                new JProperty("value", "TestObject"),
                new JProperty("children", new JArray(
                    new JObject(
                        new JProperty("name", "SomeInteger"),
                        new JProperty("value", "1337")),
                    new JObject(
                        new JProperty("name", "SomeBoolean"),
                        new JProperty("value", "True")),
                    new JObject(
                        new JProperty("name", "SomeString"),
                        new JProperty("value", "&quot;Never gonna give you up&quot;")),
                    new JObject(
                        new JProperty("name", "ChildObject"),
                        new JProperty("value", "TestObject"),
                        new JProperty("children", new JArray(
                            new JObject(
                                new JProperty("name", "SomeInteger"),
                                new JProperty("value", "58008")),
                            new JObject(
                                new JProperty("name", "SomeBoolean"),
                                new JProperty("value", "False")),
                            new JObject(
                                new JProperty("name", "SomeString"),
                                new JProperty("value", "&quot;Never gonna let you down&quot;")),
                            new JObject(
                                new JProperty("name", "ChildObject"),
                                new JProperty("value", "null")
                            )
                        ))
                    )
                )));

            ComparareJsonObject(jObject, json);
        }

        [Test]
        public void DumpIShape_DepthOne() {
            var objectDumper = new ObjectDumper(1);
            var xElement = objectDumper.Dump(new TestIShape {
                Metadata = new ShapeMetadata() {
                    Type = "TestContentType",
                    DisplayType = "Detail",
                    Alternates = new[] { "TestContentType_Detail", "TestContentType_Detail_2" },
                    Position = "1",
                    ChildContent = new HtmlString("<p>Test Para</p>"),
                    Wrappers = new[] { "TestContentType_Wrapper" }
                },
                SomeInteger = 1337,
                SomeBoolean = true,
                SomeString = "Never gonna give you up"
            }, "Model");

            Assert.Throws(typeof(NullReferenceException), () => {
                var stringBuilder = new StringBuilder();
                ObjectDumper.ConvertToJSon(xElement, stringBuilder);
                var json = stringBuilder.ToString();
            });
        }

        [Test]
        public void DumpIShape_DepthTwo() {
            var objectDumper = new ObjectDumper(2);
            var xElement = objectDumper.Dump(new TestIShape {
                Metadata = new ShapeMetadata() {
                    Type = "TestContentType",
                    DisplayType = "Detail",
                    Alternates = new[] { "TestContentType_Detail", "TestContentType_Detail_2" },
                    Position = "1",
                    ChildContent = new HtmlString("<p>Test Para</p>"),
                    Wrappers = new[] { "TestContentType_Wrapper" }
                },
                SomeInteger = 1337,
                SomeBoolean = true,
                SomeString = "Never gonna give you up"
            }, "Model");

            var stringBuilder = new StringBuilder();
            ObjectDumper.ConvertToJSon(xElement, stringBuilder);
            var json = stringBuilder.ToString();

            var jObject = new JObject(
                new JProperty("name", "Model"),
                new JProperty("value", "TestContentType Shape"));

            ComparareJsonObject(jObject, json);
        }

        [Test]
        public void DumpIShape_DepthThree() {
            var objectDumper = new ObjectDumper(3);
            var xElement = objectDumper.Dump(new TestIShape {
                Metadata = new ShapeMetadata() {
                    Type = "TestContentType",
                    DisplayType = "Detail",
                    Alternates = new[] { "TestContentType_Detail", "TestContentType_Detail_2" },
                    Position = "1",
                    ChildContent = new HtmlString("<p>Test Para</p>"),
                    Wrappers = new[] { "TestContentType_Wrapper" }
                },
                SomeInteger = 1337,
                SomeBoolean = true,
                SomeString = "Never gonna give you up"
            }, "Model");

            var stringBuilder = new StringBuilder();
            ObjectDumper.ConvertToJSon(xElement, stringBuilder);
            var json = stringBuilder.ToString();

            var jObject = new JObject(
                new JProperty("name", "Model"),
                new JProperty("value", "TestContentType Shape"));

            ComparareJsonObject(jObject, json);
        }

        [Test]
        public void DumpIShape_DepthFour() {
            var objectDumper = new ObjectDumper(4);
            var xElement = objectDumper.Dump(new TestIShape {
                Metadata = new ShapeMetadata() {
                    Type = "TestContentType",
                    DisplayType = "Detail",
                    Alternates = new[] { "TestContentType_Detail", "TestContentType_Detail_2" },
                    Position = "1",
                    ChildContent = new HtmlString("<p>Test Para</p>"),
                    Wrappers = new[] { "TestContentType_Wrapper" }
                },
                SomeInteger = 1337,
                SomeBoolean = true,
                SomeString = "Never gonna give you up"
            }, "Model");

            var stringBuilder = new StringBuilder();
            ObjectDumper.ConvertToJSon(xElement, stringBuilder);
            var json = stringBuilder.ToString();

            var jObject = new JObject(
                new JProperty("name", "Model"),
                new JProperty("value", "TestContentType Shape"));
           
            ComparareJsonObject(jObject, json);
        }

        [Test]
        public void DumpShape_DepthOne() {
            var objectDumper = new ObjectDumper(1);
            var testShape = new TestShape
            {
                Metadata = new ShapeMetadata()
                {
                    Type = "TestContentType",
                    DisplayType = "Detail",
                    Alternates = new[] { "TestContentType_Detail", "TestContentType_Detail_2" },
                    Position = "1",
                    ChildContent = new HtmlString("<p>Test Para</p>"),
                    Wrappers = new[] { "TestContentType_Wrapper" }
                },
                SomeInteger = 1337,
                SomeBoolean = true,
                SomeString = "Never gonna give you up"
            };

            testShape.Classes.Add("bodyClass1");
            testShape.Classes.Add("bodyClass2");

            testShape.Add("Child Item");

            testShape.Attributes.Add(new KeyValuePair<string, string>("onClick", "dhtmlIsBad"));

            var xElement = objectDumper.Dump(testShape, "Model");

            Assert.Throws(typeof(NullReferenceException), () =>
            {
                var stringBuilder = new StringBuilder();
                ObjectDumper.ConvertToJSon(xElement, stringBuilder);
                var json = stringBuilder.ToString();
            });
        }

        [Test]
        public void DumpShape_DepthTwo() {
            var objectDumper = new ObjectDumper(2);
            var testShape = new TestShape
            {
                Metadata = new ShapeMetadata()
                {
                    Type = "TestContentType",
                    DisplayType = "Detail",
                    Alternates = new[] { "TestContentType_Detail", "TestContentType_Detail_2" },
                    Position = "1",
                    ChildContent = new HtmlString("<p>Test Para</p>"),
                    Wrappers = new[] { "TestContentType_Wrapper" }
                },
                SomeInteger = 1337,
                SomeBoolean = true,
                SomeString = "Never gonna give you up"
            };

            testShape.Classes.Add("bodyClass1");
            testShape.Classes.Add("bodyClass2");

            testShape.Add("Child Item");

            testShape.Attributes.Add(new KeyValuePair<string, string>("onClick", "dhtmlIsBad"));

            var xElement = objectDumper.Dump(testShape, "Model");

            var stringBuilder = new StringBuilder();
            ObjectDumper.ConvertToJSon(xElement, stringBuilder);
            var json = stringBuilder.ToString();

            var jObject = new JObject(
                new JProperty("name", "Model"),
                new JProperty("value", "TestContentType Shape"));

            ComparareJsonObject(jObject, json);
        }

        [Test]
        public void DumpShape_DepthThree() {
            var objectDumper = new ObjectDumper(3);
            var testShape = new TestShape
            {
                Metadata = new ShapeMetadata()
                {
                    Type = "TestContentType",
                    DisplayType = "Detail",
                    Alternates = new[] { "TestContentType_Detail", "TestContentType_Detail_2" },
                    Position = "1",
                    ChildContent = new HtmlString("<p>Test Para</p>"),
                    Wrappers = new[] { "TestContentType_Wrapper" }
                },
                SomeInteger = 1337,
                SomeBoolean = true,
                SomeString = "Never gonna give you up"
            };

            testShape.Classes.Add("bodyClass1");
            testShape.Classes.Add("bodyClass2");

            testShape.Attributes.Add(new KeyValuePair<string, string>("onClick", "dhtmlIsBad"));

            var xElement = objectDumper.Dump(testShape, "Model");

            var stringBuilder = new StringBuilder();
            ObjectDumper.ConvertToJSon(xElement, stringBuilder);
            var json = stringBuilder.ToString();

            var jObject = new JObject(
                new JProperty("name", "Model"),
                new JProperty("value", "TestContentType Shape"),
                new JProperty("children",new JArray(
                    new JObject(
                        new JProperty("name", "Classes"),
                        new JProperty("value", "List&lt;String&gt;"),
                        new JProperty("children", new JArray(
                            new JObject(
                                new JProperty("name", "[0]"),
                                new JProperty("value", "&quot;bodyClass1&quot;")),
                            new JObject(
                                new JProperty("name", "[1]"),
                                new JProperty("value", "&quot;bodyClass2&quot;"))))),
                    new JObject(
                        new JProperty("name", "Attributes"),
                        new JProperty("value", "Dictionary&lt;String, String&gt;"),
                        new JProperty("children", new JArray(
                            new JObject(
                                new JProperty("name", "[&quot;onClick&quot;]"),
                                new JProperty("value", "&quot;dhtmlIsBad&quot;"))))),
                    new JObject(
                        new JProperty("name", "Items"),
                        new JProperty("value", "List&lt;Object&gt;")))));

            ComparareJsonObject(jObject, json);
        }

        [Test]
        public void DumpShape_DepthFour() {
            var objectDumper = new ObjectDumper(4);
            var testShape = new TestShape
            {
                Metadata = new ShapeMetadata()
                {
                    Type = "TestContentType",
                    DisplayType = "Detail",
                    Alternates = new[] { "TestContentType_Detail", "TestContentType_Detail_2" },
                    Position = "1",
                    ChildContent = new HtmlString("<p>Test Para</p>"),
                    Wrappers = new[] { "TestContentType_Wrapper" }
                },
                SomeInteger = 1337,
                SomeBoolean = true,
                SomeString = "Never gonna give you up"
            };

            testShape.Classes.Add("bodyClass1");
            testShape.Classes.Add("bodyClass2");

            testShape.Add("Child Item");

            testShape.Attributes.Add(new KeyValuePair<string, string>("onClick", "dhtmlIsBad"));

            var xElement = objectDumper.Dump(testShape, "Model");

            var stringBuilder = new StringBuilder();
            ObjectDumper.ConvertToJSon(xElement, stringBuilder);
            var json = stringBuilder.ToString();

            var jObject = new JObject(
                new JProperty("name", "Model"),
                new JProperty("value", "TestContentType Shape"),
                new JProperty("children", new JArray(
                    new JObject(
                        new JProperty("name", "Classes"),
                        new JProperty("value", "List&lt;String&gt;"),
                        new JProperty("children", new JArray(
                            new JObject(
                                new JProperty("name", "[0]"),
                                new JProperty("value", "&quot;bodyClass1&quot;")),
                            new JObject(
                                new JProperty("name", "[1]"),
                                new JProperty("value", "&quot;bodyClass2&quot;"))))),
                    new JObject(
                        new JProperty("name", "Attributes"),
                        new JProperty("value", "Dictionary&lt;String, String&gt;"),
                        new JProperty("children", new JArray(
                            new JObject(
                                new JProperty("name", "[&quot;onClick&quot;]"),
                                new JProperty("value", "&quot;dhtmlIsBad&quot;"))))),
                    new JObject(
                        new JProperty("name", "Items"),
                        new JProperty("value", "List&lt;Object&gt;"),
                        new JProperty("children", new JArray(
                            new JObject(
                                new JProperty("name", "[0]"),
                                new JProperty("value", "&quot;Child Item&quot;"))))))));

            ComparareJsonObject(jObject, json);
        }

        [Test]
        public void DumpShapeAndChild_DepthFour() {
            var objectDumper = new ObjectDumper(4);
            var testShape = new TestShape
            {
                Metadata = new ShapeMetadata
                {
                    Type = "TestContentType",
                    DisplayType = "Detail",
                    Alternates = new[] { "TestContentType_Detail", "TestContentType_Detail_2" },
                    Position = "1",
                    ChildContent = new HtmlString("<p>Test Para</p>"),
                    Wrappers = new[] { "TestContentType_Wrapper" }
                },
                SomeInteger = 1337,
                SomeBoolean = true,
                SomeString = "Never gonna give you up"
            };

            testShape.Classes.Add("bodyClass1");
            testShape.Classes.Add("bodyClass2");

            testShape.Add(new TestShape
            {
                Metadata = new ShapeMetadata
                {
                    Type = "TestContentChildType",
                    DisplayType = "Detail",
                    Alternates = new[] { "TestContentChildType_Detail", "TestContentChildType_Detail_2" },
                    Position = "1",
                    ChildContent = new HtmlString("<p>Test Child Para</p>"),
                    Wrappers = new[] { "TestContentChildType_Wrapper" }
                },
                SomeInteger = 1337,
                SomeBoolean = true,
                SomeString = "Never gonna give you up"
            });

            testShape.Attributes.Add(new KeyValuePair<string, string>("onClick", "dhtmlIsBad"));

            var xElement = objectDumper.Dump(testShape, "Model");

            var stringBuilder = new StringBuilder();
            ObjectDumper.ConvertToJSon(xElement, stringBuilder);
            var json = stringBuilder.ToString();

            var jObject = new JObject(
                new JProperty("name", "Model"),
                new JProperty("value", "TestContentType Shape"),
                new JProperty("children", new JArray(
                    new JObject(
                        new JProperty("name", "Classes"),
                        new JProperty("value", "List&lt;String&gt;"),
                        new JProperty("children", new JArray(
                            new JObject(
                                new JProperty("name", "[0]"),
                                new JProperty("value", "&quot;bodyClass1&quot;")),
                            new JObject(
                                new JProperty("name", "[1]"),
                                new JProperty("value", "&quot;bodyClass2&quot;"))))),
                    new JObject(
                        new JProperty("name", "Attributes"),
                        new JProperty("value", "Dictionary&lt;String, String&gt;"),
                        new JProperty("children", new JArray(
                            new JObject(
                                new JProperty("name", "[&quot;onClick&quot;]"),
                                new JProperty("value", "&quot;dhtmlIsBad&quot;"))))),
                    new JObject(
                        new JProperty("name", "Items"),
                        new JProperty("value", "List&lt;Object&gt;"),
                        new JProperty("children", new JArray(
                            new JObject(
                                new JProperty("name", "[0]"),
                                new JProperty("value", "TestContentChildType Shape"))))))));

            ComparareJsonObject(jObject, json);
        }

        [Test]
        public void DumpShapeAndChild_DepthSix() {
            var objectDumper = new ObjectDumper(6);
            var testShape = new TestShape
            {
                Metadata = new ShapeMetadata
                {
                    Type = "TestContentType",
                    DisplayType = "Detail",
                    Alternates = new[] { "TestContentType_Detail", "TestContentType_Detail_2" },
                    Position = "1",
                    ChildContent = new HtmlString("<p>Test Para</p>"),
                    Wrappers = new[] { "TestContentType_Wrapper" }
                },
                SomeInteger = 1337,
                SomeBoolean = true,
                SomeString = "Never gonna give you up"
            };

            testShape.Classes.Add("bodyClass1");
            testShape.Classes.Add("bodyClass2");

            testShape.Add(new TestShape
            {
                Metadata = new ShapeMetadata
                {
                    Type = "TestContentChildType",
                    DisplayType = "Detail",
                    Alternates = new[] { "TestContentChildType_Detail", "TestContentChildType_Detail_2" },
                    Position = "1",
                    ChildContent = new HtmlString("<p>Test Child Para</p>"),
                    Wrappers = new[] { "TestContentChildType_Wrapper" }
                },
                SomeInteger = 1337,
                SomeBoolean = true,
                SomeString = "Never gonna give you up"
            });

            testShape.Attributes.Add(new KeyValuePair<string, string>("onClick", "dhtmlIsBad"));

            var xElement = objectDumper.Dump(testShape, "Model");

            var stringBuilder = new StringBuilder();
            ObjectDumper.ConvertToJSon(xElement, stringBuilder);
            var json = stringBuilder.ToString();

            var jObject = new JObject(
                new JProperty("name", "Model"),
                new JProperty("value", "TestContentType Shape"),
                new JProperty("children", new JArray(
                    new JObject(
                        new JProperty("name", "Classes"),
                        new JProperty("value", "List&lt;String&gt;"),
                        new JProperty("children", new JArray(
                            new JObject(
                                new JProperty("name", "[0]"),
                                new JProperty("value", "&quot;bodyClass1&quot;")),
                            new JObject(
                                new JProperty("name", "[1]"),
                                new JProperty("value", "&quot;bodyClass2&quot;"))))),
                    new JObject(
                        new JProperty("name", "Attributes"),
                        new JProperty("value", "Dictionary&lt;String, String&gt;"),
                        new JProperty("children", new JArray(
                            new JObject(
                                new JProperty("name", "[&quot;onClick&quot;]"),
                                new JProperty("value", "&quot;dhtmlIsBad&quot;"))))),
                    new JObject(
                        new JProperty("name", "Items"),
                        new JProperty("value", "List&lt;Object&gt;"),
                        new JProperty("children", new JArray(
                            new JObject(
                                new JProperty("name", "[0]"),
                                new JProperty("value", "TestContentChildType Shape"),
                                new JProperty("children", new JArray(
                                    new JObject(
                                        new JProperty("name", "Classes"),
                                        new JProperty("value", "List&lt;String&gt;")),
                                    new JObject(
                                        new JProperty("name", "Attributes"),
                                        new JProperty("value", "Dictionary&lt;String, String&gt;")),
                                    new JObject(
                                        new JProperty("name", "Items"),
                                        new JProperty("value", "List&lt;Object&gt;")))))))))));

            ComparareJsonObject(jObject, json);
        }
    }
}
