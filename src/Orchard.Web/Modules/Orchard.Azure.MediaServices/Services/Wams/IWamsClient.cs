using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client;
using Orchard;

namespace Orchard.Azure.MediaServices.Services.Wams {
    public delegate void UpdateProgressAction(WamsUploadProgressInfo progressInfo);
    public interface IWamsClient : IDependency {
        IMediaProcessor GetLatestMediaProcessorByName(MediaProcessorName mediaProcessorName);
        IAsset GetAssetById(string assetId);
        IEnumerable<IAsset> GetAssetsById(IEnumerable<string> assetIds);
        Task<IAsset> UploadAssetAsync(string localFilePath, UpdateProgressAction updateProgress, Guid assetProgressMoniker, CancellationToken cancellationToken);
        Task DeleteAssetAsync(IAsset asset);
        Task<WamsLocators> CreateLocatorsAsync(IAsset asset, WamsLocatorCategory category);
        Task DeleteLocatorsAsync(IAsset asset, WamsLocators locators);
        IJob GetJobById(string jobId);
        IEnumerable<IJob> GetJobsById(IEnumerable<string> jobIds);
        IJob CreateNewJob(string name);
        Task<WamsAssetInfo> CreateAssetAsync(string filename);
        Task<string> GetEncoderMetadataXml(IAsset asset);
        Task<IEnumerable<string>> EnsureCorsIsEnabledAsync(params string[] origins);
    }
}
