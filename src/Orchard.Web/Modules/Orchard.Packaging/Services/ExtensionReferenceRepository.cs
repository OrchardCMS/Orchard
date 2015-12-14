using System;
using System.Collections.Generic;
using System.Linq;
using NuGet;
using Orchard.Environment.Extensions;


namespace Orchard.Packaging.Services {
    /// <summary>
    /// This repository implementation informs about what packages are already installed.
    /// </summary>
    public class ExtensionReferenceRepository : PackageRepositoryBase {
        private readonly IExtensionManager _extensionManager;
        
        public ExtensionReferenceRepository(IProjectSystem project, IPackageRepository sourceRepository, IExtensionManager extensionManager) {
            if (project == null) {
                throw new ArgumentNullException("project");
            }
            if (sourceRepository == null) {
                throw new ArgumentNullException("sourceRepository");
            }
            if (extensionManager == null) {
                throw new ArgumentNullException("extensionManager");
            }
            Project = project;
            SourceRepository = sourceRepository;
            _extensionManager = extensionManager;
        }

        private IProjectSystem Project {
            get;
            set;
        }

        private IPackageRepository SourceRepository {
            get;
            set;
        }

        public override IQueryable<IPackage> GetPackages() {
            IEnumerable<IPackage> repositoryPackages = SourceRepository.GetPackages().ToList();
            IEnumerable<IPackage> packages = from extension in _extensionManager.AvailableExtensions()
                                   let id = PackageBuilder.BuildPackageId(extension.Id, extension.ExtensionType)
                                   let version = extension.Version != null ? Version.Parse(extension.Version) : null
                                   let package = repositoryPackages.FirstOrDefault(p => p.Id == id && (version == null || p.Version == version))
                                   where package != null
                                   select package;

            return packages.AsQueryable();
        }

        public override void AddPackage(IPackage package) {}

        public override void RemovePackage(IPackage package) {}
    }
}
