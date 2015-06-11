using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Azure.MediaServices.Services.Tasks {

    /// <summary>
    /// Represents an output asset for a task.
    /// </summary>
    public class TaskOutput {

        public TaskOutput(int index, string assetType, string assetName) {
            Index = index;
            AssetType = assetType;
            AssetName = assetName;
        }

        /// <summary>
        /// Gets the position of the output asset in relation to other output assets of the same task.
        /// </summary>
        public int Index {
            get;
            private set;
        }

        /// <summary>
        /// Gets the type of the output asset.
        /// </summary>
        public string AssetType {
            get;
            private set;
        }

        /// <summary>
        /// Gets the suggested name of the output asset.
        /// </summary>
        public string AssetName {
            get;
            private set;
        }
    }
}