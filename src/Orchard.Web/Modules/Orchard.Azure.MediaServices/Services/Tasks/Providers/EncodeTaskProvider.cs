using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.Azure.MediaServices.Models;
using Orchard.Azure.MediaServices.Services.Assets;
using Orchard.Azure.MediaServices.Services.Wams;
using Orchard.Azure.MediaServices.ViewModels.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;

namespace Orchard.Azure.MediaServices.Services.Tasks.Providers {
    public class EncodeTaskProvider : TaskProviderBase {

        private const string TaskName = "Encode";

        public EncodeTaskProvider(IOrchardServices orchardServices, IAssetManager assetManager, IWamsClient wamsClient) : base(orchardServices, assetManager, wamsClient) {
        }

        public override bool CanExecute {
            get {
                var settings = _orchardServices.WorkContext.CurrentSite.As<CloudMediaSettingsPart>();
                return settings.WamsEncodingPresets.Any();
            }
        }

        public override LocalizedString Description {
            get { return T("Create a new encoding version of the cloud video item"); }
        }

        public override TaskConfiguration Editor(dynamic shapeFactory) {
            return Editor(shapeFactory, null);
        }

        public override TaskConfiguration Editor(dynamic shapeFactory, IUpdateModel updater) {
            var settings = _orchardServices.WorkContext.CurrentSite.As<CloudMediaSettingsPart>();
            var viewModel = new EncodeViewModel() {
                EncodingPresets = settings.WamsEncodingPresets,
                SelectedEncodingPreset = settings.WamsEncodingPresets.Any() ? settings.WamsEncodingPresets.ToArray()[settings.DefaultWamsEncodingPresetIndex] : null
            };

            if (updater != null) {
                updater.TryUpdateModel(viewModel, Prefix, null, null);
            }

            return new TaskConfiguration(this) {
                Settings = viewModel,
                EditorShape = shapeFactory.TaskSettingsEditor(TemplateName: TaskName, Model: viewModel, Prefix: Prefix)
            };
        }

        public override string GetDisplayText(TaskConfiguration config) {
            var viewModel = (EncodeViewModel)config.Settings;
            return viewModel.SelectedEncodingPreset;
        }

        public override TaskConnections GetConnections(TaskConfiguration config) {
            var viewModel = (EncodeViewModel)config.Settings;
            return new TaskConnections(
                new[] { new TaskInput(0, true, new[] { "VideoAsset" }) }, // Expects one video input asset.
                new[] { new TaskOutput(0, "VideoAsset", viewModel.SelectedEncodingPreset ) } // Produces one video output asset.
            );
        }

        public override ITask CreateTask(TaskConfiguration config, TaskCollection tasks, IEnumerable<IAsset> inputAssets) {
            var viewModel = (EncodeViewModel)config.Settings;

            var task = tasks.AddNew(
                viewModel.SelectedEncodingPreset,
                _wamsClient.GetLatestMediaProcessorByName(MediaProcessorName.WindowsAzureMediaEncoder),
                viewModel.SelectedEncodingPreset,
                TaskOptions.None);

            task.InputAssets.AddRange(inputAssets);
            task.OutputAssets.AddNew(Guid.NewGuid().ToString(), AssetCreationOptions.None);

            Logger.Information("New Encode task '{0}' was created.", task.Name);

            return task;
        }

        public override XElement Serialize(dynamic settings) {
            var viewModel = (EncodeViewModel)settings;
            return new XElement("EncodeTask", new XAttribute("SelectedEncodingPreset", viewModel.SelectedEncodingPreset));
        }

        public override dynamic Deserialize(XElement settingsXml) {
            var viewModel = new EncodeViewModel();

            if (settingsXml != null) {
                viewModel.SelectedEncodingPreset = settingsXml.Attr<string>("SelectedEncodingPreset");
            }

            return viewModel;
        }
    }
}