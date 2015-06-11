using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.Azure.MediaServices.Helpers;
using Orchard.Azure.MediaServices.Models;
using Orchard.Azure.MediaServices.Services.Assets;
using Orchard.Azure.MediaServices.Services.Tasks;
using Orchard.Azure.MediaServices.Services.Wams;
using Orchard.Azure.MediaServices.ViewModels;
using Microsoft.WindowsAzure.MediaServices.Client;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;

namespace Orchard.Azure.MediaServices.Services.Tasks.Providers {
    public class CreateThumbnailTaskProvider : TaskProviderBase {
        private const string TaskName = "Create Thumbnail";
        private readonly IAssetManager _assetManager;

        public CreateThumbnailTaskProvider(IAssetManager assetManager) {
            _assetManager = assetManager;
        }

        public override LocalizedString Description {
            get {
                return T("Generate a thumbnail image for the cloud video item.");
            }
        }

        public override bool CreatesVideoAsset {
            get {
                return false;
            }
        }

        public override TaskConfiguration Editor(dynamic shapeFactory) {
            return Editor(shapeFactory, null);
        }

        public override TaskConfiguration Editor(dynamic shapeFactory, IUpdateModel updater) {
            var viewModel = new CreateThumbnailViewModel();

            if (updater != null) {
                updater.TryUpdateModel(viewModel, Prefix, null, null);
            }

            return new TaskConfiguration {
                Configuration = viewModel,
                Editor = shapeFactory.TaskSettingsEditor(TemplateName: "Thumbnail", Model: viewModel, Prefix: Prefix)
            };
        }

        public override ITask CreateTask(CloudMediaContext cloudMediaContext, dynamic configuration1, TaskCollection tasks, IEnumerable<IAsset> inputAssets) {
            var processor = cloudMediaContext.GetLatestMediaProcessorByName(MediaProcessorName.WindowsAzureMediaEncoder);
            var configuration = GetXml((CreateThumbnailViewModel)configuration1);
            var task = tasks.AddNew(TaskName, processor, configuration, TaskOptions.ProtectedConfiguration);

            task.InputAssets.AddRange(inputAssets);
            task.OutputAssets.AddNew("Output asset", AssetCreationOptions.None);

            Logger.Information("New CreateThumbnail task '{0}' was created.", task.Name);

            return task;
        }

        public override int TaskChainIndex {
            get {
                return 2;
            }
        }

        public override void HandleFinishedJob(FinishedJobContext context) {
            var task = context.Job.Tasks.FirstOrDefault(x => x.Name == TaskName);

            if (task == null)
                return;

            // Delete existing thumbnail assets.
            var thumbnailAssets = context.CloudVideoPart.Assets.Where(x => x is ThumbnailAsset);
            _assetManager.RemoveAssets(thumbnailAssets);

            foreach (var asset in task.OutputAssets) {
                var closureAsset = asset;
                _assetManager.CreateAssetFor<ThumbnailAsset>(context.CloudVideoPart, a => {
                    a.UploadState.Status = AssetUploadStatus.Uploaded;
                    a.WamsAssetId = closureAsset.Id;
                });
            }
        }

        private static string GetXml(CreateThumbnailViewModel configuration) {
            var document = new XDocument(
                new XDeclaration("1.0", "utf-16", "yes"),
                new XElement("Thumbnail",
                    new XAttribute("Size", String.Format("{0},{1}", configuration.Height, configuration.Width)),
                    new XAttribute("Type", configuration.Type),
                    new XAttribute("Filename", configuration.FileName),
                    new XElement("Time", GetTimeAttributes(configuration))));

            return document.ToString(SaveOptions.DisableFormatting);
        }

        private static IEnumerable<XAttribute> GetTimeAttributes(CreateThumbnailViewModel configuration) {
            yield return new XAttribute("Value", configuration.StartTime);

            if (!String.IsNullOrWhiteSpace(configuration.Step))
                yield return new XAttribute("Step", configuration.Step);

            if (!String.IsNullOrWhiteSpace(configuration.StopTime))
                yield return new XAttribute("Stop", configuration.StopTime);
        }
    }
}