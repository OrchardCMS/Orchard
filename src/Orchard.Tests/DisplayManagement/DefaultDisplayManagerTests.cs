using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Web;
using NUnit.Framework;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;
using Orchard.Tests.Stubs;
using Orchard.Themes;

namespace Orchard.Tests.DisplayManagement {
    [TestFixture]
    public class DefaultDisplayManagerTests : ContainerTestBase {
        ShapeTable _defaultShapeTable;
        WorkContext _workContext;

        protected override void Register(Autofac.ContainerBuilder builder) {
            _defaultShapeTable = new ShapeTable { Descriptors = new Dictionary<string, ShapeDescriptor>() };
            _workContext = new TestWorkContext {
                CurrentTheme = new Theme { ThemeName = "Hello" }
            };


            builder.RegisterType<DefaultDisplayManager>().As<IDisplayManager>();
            builder.RegisterType<TestShapeTableManager>().As<IShapeTableManager>();
            builder.RegisterType<TestWorkContextAccessor>().As<IWorkContextAccessor>();
            builder.Register(ctx => _defaultShapeTable);
            builder.Register(ctx => _workContext);
        }

        public class Theme : ITheme {
            public string ThemeName { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public string Version { get; set; }
            public string Author { get; set; }
            public string HomePage { get; set; }
            public string Tags { get; set; }
            public string Zones { get; set; }
        }


        public class TestShapeTableManager : IShapeTableManager {
            private readonly ShapeTable _defaultShapeTable;

            public TestShapeTableManager(ShapeTable defaultShapeTable) {
                _defaultShapeTable = defaultShapeTable;
            }

            public ShapeTable GetShapeTable(string themeName) {
                return _defaultShapeTable;
            }
        }

        public class TestWorkContextAccessor : IWorkContextAccessor {
            private readonly WorkContext _workContext;
            public TestWorkContextAccessor(WorkContext workContext) {
                _workContext = workContext;
            }

            public WorkContext GetContext(HttpContextBase httpContext) {
                return _workContext;
            }

            public IWorkContextScope CreateWorkContextScope(HttpContextBase httpContext) {
                throw new NotImplementedException();
            }

            public WorkContext GetContext() {
                return _workContext;
            }

            public IWorkContextScope CreateWorkContextScope() {
                throw new NotImplementedException();
            }
        }

        public class TestWorkContext : WorkContext {
            readonly IDictionary<string, object> _state = new Dictionary<string, object>();
            public IContainerProvider ContainerProvider { get; set; }

            public override T Resolve<T>() {
                if (typeof(T) == typeof(ILifetimeScope))
                {
                    return (T) ContainerProvider.RequestLifetime;
                }
                
                throw new NotImplementedException();
            }

            public override T GetState<T>(string name) {
                object value;
                return _state.TryGetValue(name, out value) ? (T)value : default(T);
            }

            public override void SetState<T>(string name, T value) {
                _state[name] = value;
            }
        }

        void AddShapeDescriptor(ShapeDescriptor shapeDescriptor) {
            foreach (var binding in shapeDescriptor.Bindings) {
                _defaultShapeTable.Descriptors[binding.Key] = shapeDescriptor;
            }
        }

        static DisplayContext CreateDisplayContext(Shape shape) {
            return new DisplayContext {
                Value = shape,
                ViewContext = new ViewContext()
            };
        }

        [Test]
        public void RenderSimpleShape() {
            var displayManager = _container.Resolve<IDisplayManager>();

            var shape = new Shape {
                Metadata = new ShapeMetadata {
                    Type = "Foo"
                }
            };

            var descriptor = new ShapeDescriptor {
                ShapeType = "Foo",
            };
            descriptor.Bindings["Foo"] = new ShapeBinding {
                BindingName = "Foo",
                Binding = ctx => new HtmlString("Hi there!"),
            };
            AddShapeDescriptor(descriptor);

            var result = displayManager.Execute(CreateDisplayContext(shape));
            Assert.That(result.ToString(), Is.EqualTo("Hi there!"));
        }

        [Test]
        public void RenderFallbackShape() {
            var displayManager = _container.Resolve<IDisplayManager>();

            var shape = new Shape {
                Metadata = new ShapeMetadata {
                    Type = "Foo__2"
                }
            };

            var descriptor = new ShapeDescriptor {
                ShapeType = "Foo",
            };
            descriptor.Bindings["Foo"] = new ShapeBinding {
                BindingName = "Foo",
                Binding = ctx => new HtmlString("Hi there!"),
            };
            AddShapeDescriptor(descriptor);

            var result = displayManager.Execute(CreateDisplayContext(shape));
            Assert.That(result.ToString(), Is.EqualTo("Hi there!"));
        }

        [Test]
        public void RenderAlternateShape() {
            var displayManager = _container.Resolve<IDisplayManager>();

            var shape = new Shape {
                Metadata = new ShapeMetadata {
                    Type = "Foo__2"
                }
            };

            var descriptor = new ShapeDescriptor {
                ShapeType = "Foo",
            };
            descriptor.Bindings["Foo"] = new ShapeBinding {
                BindingName = "Foo",
                Binding = ctx => new HtmlString("Hi there!"),
            };
            descriptor.Bindings["Foo__2"] = new ShapeBinding {
                BindingName = "Foo__2",
                Binding = ctx => new HtmlString("Hello again!"),
            };
            AddShapeDescriptor(descriptor);

            var result = displayManager.Execute(CreateDisplayContext(shape));
            Assert.That(result.ToString(), Is.EqualTo("Hello again!"));
        }
    }
}
