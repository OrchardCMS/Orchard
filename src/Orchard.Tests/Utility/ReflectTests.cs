using NUnit.Framework;
using Orchard.Utility;

namespace Orchard.Tests.Utility {
    [TestFixture]
    public class ReflectTests {
        private class TestClass {
            public static int MyField;
            public static int MyField2;
            public static int MyProperty { get { MyField = 5; return MyField; } }
            public static int MyProperty2 { get { MyField2 = 5; return MyField2; } }
            public static void MyMethod(int i) { }
            public static int MyMethod(string s) { return 5; }
            public static void MyMethod2(int i) { }
            public static int MyMethod2(string s) { return 5; }
        }

        [Test]
        public void ReflectGetMemberShouldReturnCorrectMemberInfo() {
            Assert.That(Reflect.GetMember(() => TestClass.MyField).Name, Is.EqualTo("MyField"));
            Assert.That(Reflect.GetMember(() => TestClass.MyMethod(5)).Name, Is.EqualTo("MyMethod"));
        }

        [Test]
        public void ReflectShouldWorkOnFields() {
            Assert.That(Reflect.GetField(() => TestClass.MyField).Name, Is.EqualTo("MyField"));
            Assert.That(Reflect.GetField(() => TestClass.MyField2).Name, Is.EqualTo("MyField2"));
        }

        [Test]
        public void ReflectShouldWorkOnProperties() {
            Assert.That(Reflect.GetProperty(() => TestClass.MyProperty).Name, Is.EqualTo("MyProperty"));
            Assert.That(Reflect.GetProperty(() => TestClass.MyProperty2).Name, Is.EqualTo("MyProperty2"));
        }

        [Test]
        public void ReflectShouldWorkOnMethods() {
            Assert.That(Reflect.GetMethod(() => TestClass.MyMethod(5)).Name, Is.EqualTo("MyMethod"));
            Assert.That(Reflect.GetMethod(() => TestClass.MyMethod("")).Name, Is.EqualTo("MyMethod"));
            Assert.That(Reflect.GetMethod(() => TestClass.MyMethod("")).ReturnType, Is.EqualTo(typeof(int)));

            Assert.That(Reflect.GetMethod(() => TestClass.MyMethod2(5)).Name, Is.EqualTo("MyMethod2"));
            Assert.That(Reflect.GetMethod(() => TestClass.MyMethod2("")).Name, Is.EqualTo("MyMethod2"));
            Assert.That(Reflect.GetMethod(() => TestClass.MyMethod2("")).ReturnType, Is.EqualTo(typeof(int)));
        }
    }
}