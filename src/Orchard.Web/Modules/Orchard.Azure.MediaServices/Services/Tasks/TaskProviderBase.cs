using System.Collections.Generic;
using System.Xml.Linq;
using Orchard.Azure.MediaServices.Services.Assets;
using Orchard.Azure.MediaServices.Services.Wams;
using Microsoft.WindowsAzure.MediaServices.Client;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;

namespace Orchard.Azure.MediaServices.Services.Tasks {
    public abstract class TaskProviderBase : Component, ITaskProvider {

        protected readonly IOrchardServices _orchardServices;
        protected readonly IAssetManager _assetManager;
        protected readonly IWamsClient _wamsClient;

        protected TaskProviderBase(IOrchardServices orchardServices, IAssetManager assetManager, IWamsClient wamsClient) {
            _orchardServices = orchardServices;
            _assetManager = assetManager;
            _wamsClient = wamsClient;
        }

        public virtual string Name {
            get { return GetType().Name.Replace("TaskProvider", ""); }
        }

        public virtual string Prefix {
            get { return Name; }
        }

        public virtual bool CanExecute {
            get { return true; }
        }

        public abstract LocalizedString Description { get; }
        public abstract TaskConfiguration Editor(dynamic shapeFactory);
        public abstract TaskConfiguration Editor(dynamic shapeFactory, IUpdateModel updater);
        public virtual string GetDisplayText(TaskConfiguration config) {
            return null;
        }

        public abstract TaskConnections GetConnections(TaskConfiguration config);

        public abstract ITask CreateTask(TaskConfiguration config, TaskCollection tasks, IEnumerable<IAsset> inputAssets);
        //public virtual Asset CreateAssetFor(CloudVideoPart videoPart, IAsset wamsAsset, dynamic settings) {
        //    return null;
        //}

        public abstract XElement Serialize(dynamic settings);
        public abstract dynamic Deserialize(XElement settingsXml);
    }
}