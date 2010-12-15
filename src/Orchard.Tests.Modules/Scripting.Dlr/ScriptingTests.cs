using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Autofac;
using ClaySharp;
using NUnit.Framework;
using Orchard.Scripting.Dlr.Services;
using Path = Bleroy.FluentPath.Path;

namespace Orchard.Tests.Modules.Scripting.Dlr {
    [TestFixture]
    public class ScriptingTests {
        private IContainer _container;
        private IScriptingRuntime _scriptingRuntime;
        private IScriptingManager _scriptingManager;
        private readonly Path _tempFixtureFolderName = Path.Get(System.IO.Path.GetTempPath()).Combine("Orchard.Tests.Modules.Scripting");
        private Path _tempFolderName;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<RubyScriptingRuntime>().As<IScriptingRuntime>();
            builder.RegisterType<ScriptingManager>().As<IScriptingManager>();
            _container = builder.Build();
            _scriptingRuntime = _container.Resolve<IScriptingRuntime>();
            _scriptingManager = _container.Resolve<IScriptingManager>();
            _tempFolderName = _tempFixtureFolderName.Combine(System.IO.Path.GetRandomFileName());
            try {
                _tempFolderName.Delete();
            }
            catch { }
            _tempFolderName.CreateDirectory();
        }

        [TearDown]
        public void Term() {
            try { _tempFixtureFolderName.Delete(true); }
            catch { }
        }

        [Test]
        public void CreateScopeReturnsWorkingScope() {
            var scope = _scriptingRuntime.CreateScope();

            Assert.IsNotNull(scope);
            scope.SetVariable("alpha", 42);
            Assert.That(scope.GetVariable("alpha"), Is.EqualTo(42));
        }

        [Test]
        public void ScriptingManagerCanGetAndSetRubyVariables() {
            _scriptingManager.SetVariable("foo", 42);
            Assert.That(_scriptingManager.GetVariable("foo"), Is.EqualTo(42));
        }

        [Test]
        public void ScriptingManagerCanEvalExpression() {
            _scriptingManager.SetVariable("foo", 21);
            Assert.That(_scriptingManager.ExecuteExpression("foo + 21"), Is.EqualTo(42));
        }

        [Test]
        public void ScriptCanBeExecutedAndScopeProvidesContextIsolation() {
            var scriptManager1 = new ScriptingManager(_scriptingRuntime);
            var scriptManager2 = new ScriptingManager(_scriptingRuntime);

            scriptManager1.SetVariable("foo", 1);
            scriptManager2.SetVariable("foo", 2);

            var result1 = scriptManager1.ExecuteExpression("3 + foo");
            var result2 = scriptManager2.ExecuteExpression("3 + foo");

            Assert.That(result1, Is.EqualTo(4));
            Assert.That(result2, Is.EqualTo(5));
        }

        [Test]
        public void ScriptingManagerCanExecuteFile() {
            var targetPath = _tempFolderName.Combine("SampleMethodDefinition.rb");
            File.WriteAllText(targetPath, "def f\r\nreturn 32\r\nend\r\n");
            _scriptingManager.ExecuteFile(targetPath);
            Assert.That(_scriptingManager.ExecuteExpression("f / 4"), Is.EqualTo(8));
        }


        [Test]
        public void CanDeclareCallbackOnGlobalMethod() {
            _scriptingManager.SetVariable("x", new Clay(new ReturnMethodNameLengthBehavior()));

            Assert.That(_scriptingManager.ExecuteExpression("3 + x.foo()"), Is.EqualTo(6));
        }


        public class ReturnMethodNameLengthBehavior : ClayBehavior {
            public override object InvokeMemberMissing(Func<object> proceed, object self, string name, INamedEnumerable<object> args) {
                Trace.WriteLine("Returning length of " + name);
                return name.Length;
            }
        }

        [Test]
        public void CanDeclareCallbackOnInstanceEvalWithFile() {
            var targetPath = _tempFolderName.Combine("CallbackOnInstanceEval.rb");
            File.WriteAllText(targetPath, "class ExecContext\r\ndef initialize(callbacks)\r\n@callbacks = callbacks;\r\nend\r\ndef execute(text)\r\ninstance_eval(text.to_s);\r\nend\r\ndef method_missing(name, *args, &block)\r\n@callbacks.send(name, args, &block);\r\nend\r\nend\r\ndef execute(&block)\r\nExecContext.new(callbacks).instance_eval(&block);\r\nend\r\n");
            _scriptingManager.ExecuteFile(targetPath);
            _scriptingManager.SetVariable("callbacks", new CallbackApi());

            Assert.That(_scriptingManager.ExecuteExpression("execute { 1 + hello + world('yep') }"), Is.EqualTo(11));
        }

        public class CallbackApi {
            public object send(string name, IList<object> args) {
                Trace.WriteLine("Returning length of method " + name);
                return name.Length;
            }
        }
    }
}

