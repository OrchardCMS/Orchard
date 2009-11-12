using NUnit.Framework;
using Orchard.Utility;

namespace Orchard.Tests.Utility {
    [TestFixture]
    public class ReflectOnTests {
        private class TestClass {
            public int MyField;
            public int MyField2;
            public int MyProperty { get { MyField = 5; return MyField; } }
            public int MyProperty2 { get { MyField2 = 5; return MyField2; } }
            public void MyMethod(int i) { }
            public int MyMethod(string s) { return 5; }
            public void MyMethod2(int i) { }
            public int MyMethod2(string s) { return 5; }
            public TestClass MyTestClass { get { return null; } }
            public TestClass this[int i] { get { return null; } }
        }

        [Test]
        public void ReflectOnGetMemberShouldReturnCorrectMemberInfo() {
            Assert.That(ReflectOn<TestClass>.GetMember(p => p.MyField).Name, Is.EqualTo("MyField"));
            Assert.That(ReflectOn<TestClass>.GetMember(p => p.MyMethod(5)).Name, Is.EqualTo("MyMethod"));
        }

        [Test]
        public void ReflectOnShouldWorkOnFields() {
            Assert.That(ReflectOn<TestClass>.GetField(p => p.MyField).Name, Is.EqualTo("MyField"));
            Assert.That(ReflectOn<TestClass>.GetField(p => p.MyField2).Name, Is.EqualTo("MyField2"));
        }

        [Test]
        public void ReflectOnShouldWorkOnProperties() {
            Assert.That(ReflectOn<TestClass>.GetProperty(p => p.MyProperty).Name, Is.EqualTo("MyProperty"));
            Assert.That(ReflectOn<TestClass>.GetProperty(p => p.MyProperty2).Name, Is.EqualTo("MyProperty2"));
        }

        [Test]
        public void ReflectOnShouldWorkOnMethods() {
            Assert.That(ReflectOn<TestClass>.GetMethod(p => p.MyMethod(5)).Name, Is.EqualTo("MyMethod"));
            Assert.That(ReflectOn<TestClass>.GetMethod(p => p.MyMethod("")).Name, Is.EqualTo("MyMethod"));
            Assert.That(ReflectOn<TestClass>.GetMethod(p => p.MyMethod("")).ReturnType, Is.EqualTo(typeof(int)));

            Assert.That(ReflectOn<TestClass>.GetMethod(p => p.MyMethod2(5)).Name, Is.EqualTo("MyMethod2"));
            Assert.That(ReflectOn<TestClass>.GetMethod(p => p.MyMethod2("")).Name, Is.EqualTo("MyMethod2"));
            Assert.That(ReflectOn<TestClass>.GetMethod(p => p.MyMethod2("")).ReturnType, Is.EqualTo(typeof(int)));
        }

        [Test]
        public void ReflectOnShouldWorkOnDottedProperties() {
            Assert.That(ReflectOn<TestClass>.NameOf(p => p.MyTestClass.MyTestClass.MyProperty), Is.EqualTo("MyTestClass.MyTestClass.MyProperty"));
        }

        [Test]
        public void ReflectOnShouldWorkOnIndexers() {
            Assert.That(ReflectOn<TestClass>.NameOf(p => p[0].MyTestClass[1].MyProperty), Is.EqualTo("[0].MyTestClass[1].MyProperty"));
            int j = 5;
            int index = j;
            Assert.That(ReflectOn<TestClass>.NameOf(p => p.MyTestClass[index].MyProperty), Is.EqualTo("MyTestClass[5].MyProperty"));
        }
    }
}
