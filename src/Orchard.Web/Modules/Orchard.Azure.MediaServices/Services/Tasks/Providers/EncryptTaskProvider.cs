// TODO: Encryption disabled for now - uncomment this file when we need it again.
//using System;
//using System.Collections.Generic;
//using System.Xml.Linq;
//using Orchard.Azure.MediaServices.Helpers;
//using Orchard.Azure.MediaServices.Models;
//using Orchard.Azure.MediaServices.Services.Assets;
//using Orchard.Azure.MediaServices.Services.Wams;
//using Orchard.Azure.MediaServices.ViewModels.Tasks;
//using Microsoft.WindowsAzure.MediaServices.Client;
//using Orchard;
//using Orchard.ContentManagement;
//using Orchard.Localization;
//using Orchard.Logging;

//namespace Orchard.Azure.MediaServices.Services.Tasks.Providers {
//    public class EncryptTaskProvider : TaskProviderBase {

//        private const string TaskName = "Encrypt";

//        public EncryptTaskProvider(IOrchardServices orchardServices, IAssetManager assetManager, IWamsClient wamsClient) : base(orchardServices, assetManager, wamsClient) {
//        }

//        public override LocalizedString Description {
//            get { return T("Create an encrypted version of the cloud video item."); }
//        }

//        public override TaskConfiguration Editor(dynamic shapeFactory) {
//            return Editor(shapeFactory, null);
//        }

//        public override TaskConfiguration Editor(dynamic shapeFactory, IUpdateModel updater) {
//            var settings = _orchardServices.WorkContext.CurrentSite.As<CloudMediaSettingsPart>();
//            var viewModel = new EncryptViewModel {
//                KeySeedValue = settings.EncryptionKeySeedValue,
//                LicenseAcquisitionUrl = settings.EncryptionLicenseAcquisitionUrl
//            };

//            if (updater != null) {
//                updater.TryUpdateModel(viewModel, Prefix, null, null);
//            }

//            return new TaskConfiguration(this) {
//                Settings = viewModel,
//                EditorShape = shapeFactory.TaskSettingsEditor(TemplateName: TaskName, Model: viewModel, Prefix: Prefix)
//            };
//        }

//        public override TaskConnections GetConnections(TaskConfiguration config) {
//            return new TaskConnections(
//                new[] { new TaskInput(0, true, new[] { "VideoAsset" }) }, // Expects one video input asset.
//                new[] { new TaskOutput(0, "VideoAsset", "Encrypted") } // Produces one video output asset.
//            );
//        }

//        public override ITask CreateTask(TaskConfiguration config, TaskCollection tasks, IEnumerable<IAsset> inputAssets) {
//            var viewModel = (EncryptViewModel)config.Settings;

//            var task = tasks.AddNew(
//                "Encrypt",
//                _wamsClient.GetLatestMediaProcessorByName(MediaProcessorName.WindowsAzureMediaEncryptor),
//                GetConfigurationXml(viewModel),
//                TaskOptions.ProtectedConfiguration);

//            task.InputAssets.AddRange(inputAssets);
//            task.OutputAssets.AddNew(Guid.NewGuid().ToString());

//            Logger.Information("New Encrypt task '{0}' was created.", task.Name);
            
//            return task;
//        }

//        //public override Asset CreateAssetFor(CloudVideoPart videoPart, IAsset wamsAsset, dynamic settings) {
//        //    var viewModel = (EncryptViewModel)settings;
//        //    return _assetManager.CreateAssetFor<VideoAsset>(videoPart);
//        //}

//        public override XElement Serialize(dynamic settings) {
//            var viewModel = (EncryptViewModel)settings;
//            return new XElement("EncryptTask");
//        }

//        public override dynamic Deserialize(XElement settingsXml) {
//            var viewModel = new EncryptViewModel();
//            return viewModel;
//        }

//        private string GetConfigurationXml(EncryptViewModel settings) {
//            XNamespace xml = "xml";
//            XNamespace td = "http://schemas.microsoft.com/iis/media/v4/TM/TaskDefinition#";
//            var document = new XDocument(
//                new XDeclaration("1.0", "utf-16", "yes"),
//                new XElement(td + "taskDefinition",
//                    new XElement("name", "PlayReady Protection"),
//                    new XElement("id", "9A3BFEAC-F8AE-41CA-87FA-D639E4D1C753"),
//                    new XElement("description", new XAttribute(xml + "lang", "en")),
//                    new XElement("properties",
//                        new XAttribute("namespace", "http://schemas.microsoft.com/iis/media/v4/SharedData#"),
//                        new XAttribute("prefix", "sd"),
//                        XPropertyElement("adjustSubSamples", settings.AdjustSubSamples),
//                        XPropertyElement("contentKey", settings.ContentKey.TrimSafe()),
//                        XPropertyElement("customAttributes", settings.CustomAttributes.TrimSafe()),
//                        XPropertyElement("dataFormats", settings.DataFormats.TrimSafe()),
//                        XPropertyElement("keyId", settings.DataFormats.TrimSafe()),
//                        XPropertyElement("keySeedValue", settings.KeySeedValue.TrimSafe()),
//                        XPropertyElement("licenseAcquisitionUrl", settings.LicenseAcquisitionUrl.TrimSafe()),
//                        XPropertyElement("useSencBox", settings.UseSencBox),
//                        XPropertyElement("serviceId", settings.ServiceId.TrimSafe())),
//                    new XElement("inputFolder"),
//                    new XElement("outputFolder", "Protected"),
//                    new XElement("taskCode",
//                        new XElement("type", "Microsoft.Web.Media.TransformManager.DigitalRightsManagementTask, Microsoft.Web.Media.TransformManager.DigitalRightsManagement, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"))));

//            return document.ToString(SaveOptions.DisableFormatting);
//        }

//        private XElement XPropertyElement(string name, object value) {
//            return value != null ? new XElement("property", new XAttribute("name", name), new XAttribute("value", value)) : null;
//        }
//    }
//}