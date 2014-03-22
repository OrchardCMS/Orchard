using Autofac;
using Microsoft.CloudMedia.Models;
using Microsoft.CloudMedia.Models.Assets;
using Microsoft.CloudMedia.Models.Records;
using Microsoft.CloudMedia.Services.Assets;
using Microsoft.CloudMedia.Tests.Common;
using Moq;
using NUnit.Framework;
using Orchard.Data;
using Orchard.Tests.Utility;

namespace Microsoft.CloudMedia.Tests.Services {
    [TestFixture]
    public class AzureMediaManagerTests {
        private IContainer _container;
        private IRepository<AssetRecord> _stubAssetRepository;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            var assetFactoryMock = new Mock<IAssetFactory>();

            _stubAssetRepository = new StubRepository<AssetRecord>();
            assetFactoryMock.Setup(x => x.Create(It.IsAny<string>())).Returns(new VideoAsset());
            
            builder.RegisterInstance(_stubAssetRepository);
            builder.RegisterInstance(assetFactoryMock.Object);
            builder.RegisterType<AssetManager>().As<IAssetManager>();
            builder.RegisterAutoMocking(MockBehavior.Loose);

            _container = builder.Build();
        }

        [Test]
        public void RemovingAssetsInProgressAreMarkedAsToBeRemoved() {
            var azureMediaManager = _container.Resolve<IAssetManager>();
            var videoPart = PartFactory.CreatePart<CloudVideoPart, CloudVideoPartRecord>();

            videoPart._assetManager = _container.Resolve<IAssetManager>();
            FillAssetRepository(videoPart);
            azureMediaManager.DeleteAssetsFor(videoPart);

            foreach (var asset in videoPart.Assets) {
                Assert.That(asset.UploadState.Status, Is.EqualTo(AssetUploadStatus.Canceled));
            }
        }

        private void FillAssetRepository(CloudVideoPart videoPart) {
            var asset1 = CreateVideoAssetRecord(videoPart);
            var asset2 = CreateVideoAssetRecord(videoPart);

            asset1.PublishStatus = AssetPublishStatus.None;
            asset1.UploadStatus = AssetUploadStatus.Pending;
            asset2.PublishStatus = AssetPublishStatus.Published;
            asset2.UploadStatus = AssetUploadStatus.Uploading;
        }

        private AssetRecord CreateVideoAssetRecord(CloudVideoPart videoPart) {
            var asset = new AssetRecord {
                VideoPartRecord = videoPart.Record,
                PublishStatus = AssetPublishStatus.Published,
                UploadStatus = AssetUploadStatus.Uploading,
                Type = "Video"
            };

            _stubAssetRepository.Create(asset);
            return asset;
        }
    }
}