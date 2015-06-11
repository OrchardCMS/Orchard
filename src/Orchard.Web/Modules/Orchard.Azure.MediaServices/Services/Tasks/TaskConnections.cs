using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Azure.MediaServices.Services.Tasks {
	/// <summary>
	/// Describes the input and output assets of a task.
	/// </summary>
	public class TaskConnections {

		public TaskConnections(IEnumerable<TaskInput> inputs, IEnumerable<TaskOutput> outputs) {
			Inputs = inputs;
			Outputs = outputs;
		}

		/// <summary>
		/// Gets of list of TaskInput objects representing the input assets of the task.
		/// </summary>
		public IEnumerable<TaskInput> Inputs {
			get;
			private set;
		}

		/// <summary>
		/// Gets of list of TaskOutput objects representing the output assets of the task.
		/// </summary>
		public IEnumerable<TaskOutput> Outputs {
			get;
			private set;
		}
	}
}