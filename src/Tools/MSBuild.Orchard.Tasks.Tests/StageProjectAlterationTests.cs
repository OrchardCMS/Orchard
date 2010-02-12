using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Moq;
using MSBuild.Orchard.Tasks.Tests.TestData;
using NUnit.Framework;

namespace MSBuild.Orchard.Tasks.Tests {
    [TestFixture]
    public class StageProjectAlterationTests {
        private TestDataFiles _testDataFiles;
        private StageProjectAlteration _task;
        private string _xmlns = "http://schemas.microsoft.com/developer/msbuild/2003";

        [SetUp]
        public void Init() {
            _testDataFiles = new TestDataFiles();
            _task = new StageProjectAlteration();

            var engine = new Mock<IBuildEngine>();
            _task.BuildEngine = engine.Object;
        }

        [TearDown]
        public void Term() {
            _testDataFiles.Dispose();
            _testDataFiles = null;
        }

        [Test]
        public void ClassShouldBeCallable() {
            var result = _task.Execute();
            Assert.That(result, Is.False);
        }

        [Test]
        public void ProjectFileNameMustExist() {
            _task.ProjectFileName = "no-such-file.csproj";
            var result = _task.Execute();
            Assert.That(result, Is.False);

            _task.ProjectFileName = _testDataFiles.Get("SimpleWebProject.xml");
            result = _task.Execute();
            Assert.That(result, Is.True);
        }

        [Test]
        public void ProjectReferencesMustChange() {
            _task.ProjectFileName = _testDataFiles.Get("ProjectReferences.xml");
            var before = XDocument.Load(_task.ProjectFileName);
            var result = _task.Execute();
            var after = XDocument.Load(_task.ProjectFileName);

            Assert.That(result, Is.True);

            var beforeProjectReferences = before
                .Elements(XName.Get("Project", _xmlns))
                .Elements(XName.Get("ItemGroup", _xmlns))
                .Elements(XName.Get("ProjectReference", _xmlns));

            Assert.That(beforeProjectReferences.Count(), Is.Not.EqualTo(0));

            var afterProjectReferences = after
                .Elements(XName.Get("Project", _xmlns))
                .Elements(XName.Get("ItemGroup", _xmlns))
                .Elements(XName.Get("ProjectReference", _xmlns));

            Assert.That(afterProjectReferences.Count(), Is.EqualTo(0));
        }

        [Test]
        public void ExtraFilesAreDetected() {
            _task.ProjectFileName = _testDataFiles.Get("ExtraFiles.xml");
            var result = _task.Execute();
            Assert.That(result, Is.True);

            Assert.That(_task.ExtraFiles.Count(), Is.EqualTo(5));
        }

        [Test]
        public void ContentFilesAreDetected() {
            _task.ProjectFileName = _testDataFiles.Get("ExtraFiles.xml");
            _task.AddContentFiles = new[] {
                                              new TaskItem("newfile.xml"),
                                              new TaskItem("another\\newfile.txt")
                                          };
            var result = _task.Execute();
            Assert.That(result, Is.True);

            var after = XDocument.Load(_task.ProjectFileName);
            var afterIncludes = after
                .Elements(XName.Get("Project", _xmlns))
                .Elements(XName.Get("ItemGroup", _xmlns))
                .Elements(XName.Get("Content", _xmlns))
                .Attributes("Include")
                .Select(attr => (string)attr)
                .ToArray();

            Assert.That(afterIncludes, Has.Some.EqualTo("newfile.xml"));
            Assert.That(afterIncludes, Has.Some.EqualTo("another\\newfile.txt"));
        }
    }
}
