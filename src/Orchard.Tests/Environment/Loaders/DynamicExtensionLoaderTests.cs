using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions.Compilers;
using Orchard.Environment.Extensions.Loaders;
using Orchard.FileSystems.AppData;
using Orchard.FileSystems.Dependencies;
using Orchard.FileSystems.VirtualPath;
using Orchard.Services;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Environment.Loaders {
    [TestFixture]
    public class DynamicExtensionLoaderTests {
        private IContainer _container;
        private Mock<IProjectFileParser> _mockedStubProjectFileParser;
        private Mock<IDependenciesFolder> _mockedDependenciesFolder;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<DynamicExtensionLoaderAccessor>().As<DynamicExtensionLoaderAccessor>();

            builder.RegisterType<DefaultBuildManager>().As<IBuildManager>();
            builder.RegisterType<DefaultAssemblyProbingFolder>().As<IAssemblyProbingFolder>();
            builder.RegisterType<StubHostEnvironment>().As<IHostEnvironment>();
            builder.RegisterType<StubVirtualPathMonitor>().As<IVirtualPathMonitor>();
            builder.RegisterType<StubVirtualPathProvider>().As<IVirtualPathProvider>();
            builder.RegisterType<DefaultAssemblyLoader>().As<IAssemblyLoader>();
            builder.RegisterType<StubClock>().As<IClock>();
            builder.RegisterType<StubAppDataFolder>().As<IAppDataFolder>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();

            builder.RegisterInstance(new ExtensionLocations());

            _mockedStubProjectFileParser = new Mock<IProjectFileParser>();
            builder.RegisterInstance(_mockedStubProjectFileParser.Object).As<IProjectFileParser>();
            builder.RegisterInstance(new StubFileSystem(new StubClock())).As<StubFileSystem>();

            _mockedDependenciesFolder = new Mock<IDependenciesFolder>();
            builder.RegisterInstance(_mockedDependenciesFolder.Object).As<IDependenciesFolder>();

            _container = builder.Build();
        }

        [Test]
        public void GetDependenciesContainsNoDuplicatesTest() {
            const string pathPrefix = "~/modules/foo";
            const string projectName = "orchard.a.csproj";
            const string fileName1 = "a.cs";
            const string fileName2 = "b.cs";

            var vpp = _container.Resolve<IVirtualPathProvider>();
            var projectPath = vpp.Combine(pathPrefix, projectName);
            using (vpp.CreateText(projectPath)) { }

            // Create duplicate source files (invalid situation in reality but easy enough to test)
            _mockedStubProjectFileParser.Setup(stubProjectFileParser => stubProjectFileParser.Parse(It.IsAny<string>())).Returns(
                new ProjectFileDescriptor { SourceFilenames = new[] { fileName1, fileName2, fileName1 } }); // duplicate file

            var extensionLoader = _container.Resolve<DynamicExtensionLoaderAccessor>();
            var dependencies = extensionLoader.GetDependenciesAccessor(projectPath);

            Assert.That(dependencies.Count(), Is.EqualTo(3), "3 results should mean no duplicates");
            Assert.That(dependencies.FirstOrDefault(dep => dep.Equals(projectPath)), Is.Not.Null);
            Assert.That(dependencies.FirstOrDefault(dep => dep.Equals(vpp.Combine(pathPrefix, fileName1))), Is.Not.Null);
            Assert.That(dependencies.FirstOrDefault(dep => dep.Equals(vpp.Combine(pathPrefix, fileName2))), Is.Not.Null);
        }

        [Test]
        public void GetDependenciesContainsNoDuplicatesEvenIfMultipleProjectsTest() {
            const string path1Prefix = "~/modules/foo";
            const string path2Prefix = "~/modules/bar";
            const string path3Prefix = "~/modules/blah";
            const string project1Name = "orchard.a.csproj";
            const string project2Name = "orchard.b.csproj";
            const string project3Name = "orchard.c.csproj";
            const string fileName1 = "a.cs";
            const string fileName2 = "b.cs";
            const string commonFileName = "c.cs";

            var vpp = _container.Resolve<IVirtualPathProvider>();

            var project1Path = vpp.Combine(path1Prefix, project1Name);
            using (vpp.CreateText(project1Path)) { }

            var project2Path = vpp.Combine(path2Prefix, project2Name);
            using (vpp.CreateText(project2Path)) { }

            var project3Path = vpp.Combine(path3Prefix, project3Name);
            using (vpp.CreateText(project3Path)) { }


            // Project a reference b and c which share a file in common

            // Result for project a
            _mockedStubProjectFileParser
                .Setup(stubProjectFileParser => stubProjectFileParser.Parse(It.Is<string>(virtualPath => virtualPath == project1Path)))
                .Returns(
                    new ProjectFileDescriptor {
                        SourceFilenames = new[] {fileName1, fileName2},
                        References = new[] {
                            new ReferenceDescriptor {
                                ReferenceType = ReferenceType.Project,
                                SimpleName = project2Path,
                                FullName = project2Path,
                                Path = "..\\bar\\" + project2Name
                            },
                            new ReferenceDescriptor {
                                ReferenceType = ReferenceType.Project,
                                SimpleName = project3Path,
                                FullName = project3Path,
                                Path = "..\\blah\\" + project3Name
                            }
                        }
                    });

            // Result for project b and c
            _mockedStubProjectFileParser
                .Setup(stubProjectFileParser => stubProjectFileParser.Parse(It.Is<string>(virtualPath => (virtualPath == project2Path || virtualPath == project3Path))))
                .Returns(
                    new ProjectFileDescriptor {
                        SourceFilenames = new[] { commonFileName }
                    });

            var extensionLoader = _container.Resolve<DynamicExtensionLoaderAccessor>();
            var dependencies = extensionLoader.GetDependenciesAccessor(project1Path);

            Assert.That(dependencies.Count(), Is.EqualTo(7), "7 results should mean no duplicates");

            // Project files
            Assert.That(dependencies.FirstOrDefault(dep => dep.Contains(project1Path)), Is.Not.Null);
            Assert.That(dependencies.FirstOrDefault(dep => dep.Contains(project2Path)), Is.Not.Null);
            Assert.That(dependencies.FirstOrDefault(dep => dep.Contains(project3Path)), Is.Not.Null);

            // Individual source files
            Assert.That(dependencies.FirstOrDefault(dep => dep.Contains(vpp.Combine(path1Prefix, fileName1))), Is.Not.Null);
            Assert.That(dependencies.FirstOrDefault(dep => dep.Contains(vpp.Combine(path1Prefix, fileName2))), Is.Not.Null);
            Assert.That(dependencies.FirstOrDefault(dep => dep.Contains(vpp.Combine(path2Prefix, commonFileName))), Is.Not.Null);
            Assert.That(dependencies.FirstOrDefault(dep => dep.Contains(vpp.Combine(path3Prefix, commonFileName))), Is.Not.Null);
        }

        internal class DynamicExtensionLoaderAccessor : DynamicExtensionLoader {
            public DynamicExtensionLoaderAccessor(
                IBuildManager buildManager,
                IVirtualPathProvider virtualPathProvider,
                IVirtualPathMonitor virtualPathMonitor,
                IHostEnvironment hostEnvironment,
                IAssemblyProbingFolder assemblyProbingFolder,
                IDependenciesFolder dependenciesFolder,
                IProjectFileParser projectFileParser,
                ExtensionLocations extensionLocations)
                : base(buildManager, virtualPathProvider, virtualPathMonitor, hostEnvironment, assemblyProbingFolder, dependenciesFolder, projectFileParser, extensionLocations) {}

            public IEnumerable<string> GetDependenciesAccessor(string projectPath) {
                return GetDependencies(projectPath);
            }
        }
    }
}
