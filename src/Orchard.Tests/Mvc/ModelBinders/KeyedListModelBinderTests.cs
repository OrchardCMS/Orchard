using System.Collections.Generic;
using System.Web.Mvc;
using NUnit.Framework;
using Orchard.Mvc.ModelBinders;

namespace Orchard.Tests.Mvc.ModelBinders {
    [TestFixture]
    public class KeyedListModelBinderTests {
        private class Foo {
            public string Name { get; set; }
            public string Value { get; set; }
        }
        [Test]
        public void BinderShouldBindValues() {
            var controllerContext = new ControllerContext();

            var binders = new ModelBinderDictionary {
                { typeof(Foo), new DefaultModelBinder() } 
            };

            var input = new FormCollection { 
                { "fooInstance[Bar1].Value", "bar1value" }, 
                { "fooInstance[Bar2].Value", "bar2value" } 
            };

            var foos = new[] {
                new Foo {Name = "Bar1", Value = "uno"},
                new Foo {Name = "Bar2", Value = "dos"},
                new Foo {Name = "Bar3", Value = "tres"},
            };

            var providers = new EmptyModelMetadataProvider();

            var bindingContext = new ModelBindingContext {
                ModelMetadata = providers.GetMetadataForType(() => foos, foos.GetType()),
                ModelName = "fooInstance", 
                ValueProvider = input.ToValueProvider() 
            };

            var binder = new KeyedListModelBinder<Foo>(binders, foo => foo.Name);

            var result = (IList<Foo>)binder.BindModel(controllerContext, bindingContext);

            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0].Value, Is.EqualTo("bar1value"));
            Assert.That(result[1].Value, Is.EqualTo("bar2value"));
        }
    }
}