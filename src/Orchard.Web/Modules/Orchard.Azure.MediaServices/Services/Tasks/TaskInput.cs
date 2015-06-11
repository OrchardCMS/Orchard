using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Azure.MediaServices.Services.Tasks {

	/// <summary>
	/// Represents an input asset for a task.
	/// </summary>
	public class TaskInput {

		public TaskInput(int index, bool isRequired, IEnumerable<string> supportedAssetTypes) {
			Index = index;
			IsRequired = isRequired;
			SupportedAssetTypes = supportedAssetTypes;
		}

		/// <summary>
		/// Gets the position of the input asset in relation to other input assets of the same task.
		/// </summary>
		public int Index {
			get;
			private set;
		}

		/// <summary>
		/// Gets a boolean indicating whether the input asset is required for the task to operate.
		/// </summary>
		public bool IsRequired {
			get;
			private set;
		}

		/// <summary>
		/// Gets the supported types for the input asset.
		/// </summary>
		public IEnumerable<string> SupportedAssetTypes {
			get;
			private set;
		}
	}
}