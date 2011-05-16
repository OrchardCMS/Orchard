using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.Environment;
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

            _mockedStubProjectFileParser = new Mock<IProjectFileParser>();
            builder.RegisterInstance(_mockedStubProjectFileParser.Object).As<IProjectFileParser>();
            builder.RegisterInstance(new StubFileSystem(new StubClock())).As<StubFileSystem>();

            _mockedDependenciesFolder = new Mock<IDependenciesFolder>();
            builder.RegisterInstance(_mockedDependenciesFolder.Object).As<IDependenciesFolder>();

            _container = builder.Build();
        }

        [Test]
        public void GetDependenciesContainsNoDuplicatesTest() {
            const string fileName1 = "a.cs";
            const string fileName2 = "b.cs";

            DynamicExtensionLoaderAccessor extensionLoader = _container.Resolve<DynamicExtensionLoaderAccessor>();
            StubFileSystem stubFileSystem = _container.Resolve<StubFileSystem>();
            StubFileSystem.FileEntry fileEntry = stubFileSystem.CreateFileEntry("orchard.a.csproj");

            // Create duplicate source files (invalid situation in reality but easy enough to test)
            _mockedStubProjectFileParser.Setup(stubProjectFileParser => stubProjectFileParser.Parse(It.IsAny<string>())).Returns(
                new ProjectFileDescriptor { SourceFilenames = new[] { fileName1, fileName2, fileName1 } }); // duplicate file

            IEnumerable<string> dependencies = extensionLoader.GetDependenciesAccessor(fileEntry.Name);

            Assert.That(dependencies.Count(), Is.EqualTo(3), "3 results should mean no duplicates");
            Assert.That(dependencies.FirstOrDefault(dep => dep.Equals(fileEntry.Name)), Is.Not.Null);
            Assert.That(dependencies.FirstOrDefault(dep => dep.Equals(fileName1)), Is.Not.Null);
            Assert.That(dependencies.FirstOrDefault(dep => dep.Equals(fileName2)), Is.Not.Null);
        }

        [Test]
        public void GetDependenciesContainsNoDuplicatesEvenIfMultipleProjectsTest() {
            const string fileName1 = "a.cs";
            const string fileName2 = "b.cs";

            const string commonFileName = "c.cs";

            DynamicExtensionLoaderAccessor extensionLoader = _container.Resolve<DynamicExtensionLoaderAccessor>();
            StubFileSystem stubFileSystem = _container.Resolve<StubFileSystem>();
            StubFileSystem.FileEntry fileEntry = stubFileSystem.CreateFileEntry("orchard.a.csproj");
            StubFileSystem.FileEntry fileEntry2 = stubFileSystem.CreateFileEntry("orchard.b.csproj");
            StubFileSystem.FileEntry fileEntry3 = stubFileSystem.CreateFileEntry("orchard.c.csproj");

            // Project a reference b and c which share a file in common

            // Result for project a
            _mockedStubProjectFileParser
                .Setup(stubProjectFileParser => stubProjectFileParser.Parse(It.Is<string>(virtualPath => virtualPath == "orchard.a.csproj")))
                .Returns(
                    new ProjectFileDescriptor {
                        SourceFilenames = new[] { fileName1, fileName2 },
                        References = new[] { 
                            new ReferenceDescriptor {
                                ReferenceType = ReferenceType.Project,
                                SimpleName = Path.GetFileNameWithoutExtension(fileEntry2.Name),
                                FullName = Path.GetFileNameWithoutExtension(fileEntry2.Name),
                                Path = fileEntry2.Name
                            },
                            new ReferenceDescriptor {
                                ReferenceType = ReferenceType.Project,
                                SimpleName = Path.GetFileNameWithoutExtension(fileEntry3.Name),
                                FullName = Path.GetFileNameWithoutExtension(fileEntry3.Name),
                                Path = fileEntry3.Name
                            }
                        }
                    });

            // Result for project b and c
            _mockedStubProjectFileParser
                .Setup(stubProjectFileParser => stubProjectFileParser.Parse(It.Is<string>(virtualPath => (virtualPath == "~/orchard.b.csproj" || virtualPath == "~/orchard.c.csproj"))))
                .Returns(
                    new ProjectFileDescriptor {
                        SourceFilenames = new[] { commonFileName }
                    });

            IEnumerable<string> dependencies = extensionLoader.GetDependenciesAccessor(fileEntry.Name);

            Assert.That(dependencies.Count(), Is.EqualTo(6), "6 results should mean no duplicates");

            // Project files
            Assert.That(dependencies.FirstOrDefault(dep => dep.Contains(fileEntry.Name)), Is.Not.Null);
            Assert.That(dependencies.FirstOrDefault(dep => dep.Contains(fileEntry2.Name)), Is.Not.Null);
            Assert.That(dependencies.FirstOrDefault(dep => dep.Contains(fileEntry3.Name)), Is.Not.Null);

            // Individual source files
            Assert.That(dependencies.FirstOrDefault(dep => dep.Contains(fileName1)), Is.Not.Null);
            Assert.That(dependencies.FirstOrDefault(dep => dep.Contains(fileName2)), Is.Not.Null);
            Assert.That(dependencies.FirstOrDefault(dep => dep.Contains(commonFileName)), Is.Not.Null);
        }

        [Test]
        public void GetDependenciesContainsBinForReferencedProjectsTest() {
            const string fileName1 = "a.cs";
            const string fileName2 = "b.cs";

            const string commonFileName = "c.cs";

            DynamicExtensionLoaderAccessor extensionLoader = _container.Resolve<DynamicExtensionLoaderAccessor>();
            StubFileSystem stubFileSystem = _container.Resolve<StubFileSystem>();
            StubFileSystem.FileEntry fileEntry = stubFileSystem.CreateFileEntry("orchard.a.csproj");
            StubFileSystem.FileEntry fileEntry2 = stubFileSystem.CreateFileEntry("orchard.b.csproj");

            StubFileSystem.DirectoryEntry directoryEntry = stubFileSystem.CreateDirectoryEntry("bin");
            StubFileSystem.FileEntry fileEntry3 = directoryEntry.CreateFile("orchard.b.dll");

            // Project a reference b and c which share a file in common

            // Result for project a
            _mockedStubProjectFileParser
                .Setup(stubProjectFileParser => stubProjectFileParser.Parse(It.Is<string>(virtualPath => virtualPath == "orchard.a.csproj")))
                .Returns(
                    new ProjectFileDescriptor {
                        SourceFilenames = new[] { fileName1, fileName2 },
                        References = new[] { 
                            new ReferenceDescriptor {
                                ReferenceType = ReferenceType.Project,
                                SimpleName = Path.GetFileNameWithoutExtension(fileEntry2.Name),
                                FullName = Path.GetFileNameWithoutExtension(fileEntry2.Name),
                                Path = fileEntry2.Name
                            }
                        }
                    });

            // Result for project b and c
            _mockedStubProjectFileParser
                .Setup(stubProjectFileParser => stubProjectFileParser.Parse(It.Is<string>(virtualPath => virtualPath == "~/orchard.b.csproj")))
                .Returns(
                    new ProjectFileDescriptor {
                        SourceFilenames = new[] { commonFileName }
                    });

            _mockedDependenciesFolder
                .Setup(dependenciesFolder => dependenciesFolder.GetDescriptor(It.Is<string>(moduleName => moduleName == Path.GetDirectoryName(fileEntry2.Name))))
                .Returns(
                    new DependencyDescriptor {
                        VirtualPath = Path.Combine(directoryEntry.Name, fileEntry3.Name)
                    });

            IEnumerable<string> dependencies = extensionLoader.GetDependenciesAccessor(fileEntry.Name);

            Assert.That(dependencies.Count(), Is.EqualTo(6), "6 results should mean no duplicates");

            // Project files
            Assert.That(dependencies.FirstOrDefault(dep => dep.Contains(fileEntry.Name)), Is.Not.Null);
            Assert.That(dependencies.FirstOrDefault(dep => dep.Contains(fileEntry2.Name)), Is.Not.Null);
            Assert.That(dependencies.FirstOrDefault(dep => dep.Contains(Path.Combine(directoryEntry.Name, fileEntry3.Name))), Is.Not.Null);

            // Individual source files
            Assert.That(dependencies.FirstOrDefault(dep => dep.Contains(fileName1)), Is.Not.Null);
            Assert.That(dependencies.FirstOrDefault(dep => dep.Contains(fileName2)), Is.Not.Null);
            Assert.That(dependencies.FirstOrDefault(dep => dep.Contains(commonFileName)), Is.Not.Null);
        }

        internal class DynamicExtensionLoaderAccessor : DynamicExtensionLoader {
            public DynamicExtensionLoaderAccessor(
                IBuildManager buildManager,
                IVirtualPathProvider virtualPathProvider,
                IVirtualPathMonitor virtualPathMonitor,
                IHostEnvironment hostEnvironment,
                IAssemblyProbingFolder assemblyProbingFolder,
                IDependenciesFolder dependenciesFolder,
                IProjectFileParser projectFileParser)
                : base(buildManager, virtualPathProvider, virtualPathMonitor, hostEnvironment, assemblyProbingFolder, dependenciesFolder, projectFileParser) {}

            public IEnumerable<string> GetDependenciesAccessor(string projectPath) {
                return GetDependencies(projectPath);
            }
        }
    }
}
