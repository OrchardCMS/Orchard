using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.WindowsAzure.MediaServices.Client;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;

namespace Orchard.Azure.MediaServices.Services.Tasks {
    public interface ITaskProvider : IDependency {
        /// <summary>
        /// Gets the technical name of the task provider.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a string used for prefixing form values in the UI emitted by the task provider.
        /// </summary>
        string Prefix { get; }

        /// <summary>
        /// Gets a boolean indicating whether this task provider can execute given the current configuration and cirmumstances.
        /// </summary>
        bool CanExecute { get; }

        /// <summary>
        /// Gets the text to display in the UI when listing this task provider.
        /// </summary>
        LocalizedString Description { get; }

        /// <summary>
        /// Creates a shape used to render the editor UI for the task provider.
        /// </summary>
        TaskConfiguration Editor(dynamic shapeFactory);

        /// <summary>
        /// Processes the input submitted from the task provider editor UI.
        /// </summary>
        TaskConfiguration Editor(dynamic shapeFactory, IUpdateModel updater);

        /// <summary>
        /// Gets the text to display based on the configuration. This is typically used to generate a name for a job to be created.
        /// </summary>
        string GetDisplayText(TaskConfiguration config);

        /// <summary>
        /// Returns information about the supported input/output assets of a task with the specified configuration.
        /// </summary>
        /// <param name="config">The task configuration based on which the supported input/output assets should be determined.</param>
        TaskConnections GetConnections(TaskConfiguration config);

        /// <summary>
        /// Creates a WAMS task (an ITask instance) based on the the specified task configuration and input assets.
        /// </summary>
        /// <param name="config">The task configuration based on which the new task should be created.</param>
        /// <param name="tasks">A WAMS TaskCollection object to be used to instantiate new tasks.</param>
        /// <param name="inputAssets">A list of assets which constitute the input of the task.</param>
        ITask CreateTask(TaskConfiguration config, TaskCollection tasks, IEnumerable<IAsset> inputAssets);

        /// <summary>
        /// Creates an asset based on the specified WAMS asset that was created as part of a WAMS task.
        /// </summary>
        /// <param name="videoPart">The CloudVideoPart to associate the asset with.</param>
        /// <param name="wamsAsset">The WAMS asset (IAsset) that was created by WAMS.</param>
        /// <param name="settings">The task provider settings when the task was created.</param>
        //Asset CreateAssetFor(CloudVideoPart videoPart, IAsset wamsAsset, dynamic settings);

        XElement Serialize(dynamic settings);
        dynamic Deserialize(XElement settingsXml);
    }
}
