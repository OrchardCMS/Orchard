using System.IO;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.Media;
using Orchard.Azure.FileSystems.Media;

namespace Orchard.AzureBlobStorage.Services {
	[OrchardSuppressDependency("Orchard.FileSystems.Media.FileSystemStorageProvider")]
	public class AzureBlobStorageProviderDependency : AzureBlobStorageProvider {
		public AzureBlobStorageProviderDependency(ShellSettings shellSettings, IMimeTypeProvider mimeTypeProvider) : base(shellSettings, mimeTypeProvider) { }
	}
}