using System;
using System.Linq;
using System.Collections.Generic;
using Orchard.Azure.MediaServices.Helpers;
using Orchard.Azure.MediaServices.Models.Records;
using Orchard.ContentManagement.Utilities;

namespace Orchard.Azure.MediaServices.Models.Jobs {
	public class Job {
		internal readonly LazyField<IEnumerable<Task>> _tasksField = new LazyField<IEnumerable<Task>>();
		internal readonly LazyField<CloudVideoPart> _cloudVideoPartField = new LazyField<CloudVideoPart>();

		public Job(JobRecord record) {
			Record = record;
		}

		public JobRecord Record { get; private set; }

		public IEnumerable<Task> Tasks {
			get { return _tasksField.Value; }
		}

		public CloudVideoPart CloudVideoPart {
			get { return _cloudVideoPartField.Value; }
		}

		public string WamsJobId {
			get { return Record.WamsJobId; }
			set { Record.WamsJobId = value; }
		}

		public string Name {
			get { return Record.Name; }
			set { Record.Name = value; }
		}

		public string Description {
			get { return Record.Description; }
			set { Record.Description = value; }
		}

		public JobStatus Status {
			get { return Record.Status; }
			set { Record.Status = value; }
		}

		public DateTime? CreatedUtc {
			get { return Record.CreatedUtc; }
			set { Record.CreatedUtc = value; }
		}

		public DateTime? StartedUtc {
			get { return Record.StartedUtc; }
			set { Record.StartedUtc = value; }
		}

		public DateTime? FinishedUtc {
			get { return Record.FinishedUtc; }
			set { Record.FinishedUtc = value; }
		}

		public string ErrorMessage {
			get { return Record.ErrorMessage; }
			set { Record.ErrorMessage = value; }
		}

		public string OutputAssetName {
			get { return Record.OutputAssetName; }
			set { Record.OutputAssetName = value; }
		}

		public string OutputAssetDescription {
			get { return Record.OutputAssetDescription; }
			set { Record.OutputAssetDescription = value; }
		}

		public int PercentComplete {
			get {
				if (Tasks != null && Tasks.Any())
					return (int)Tasks.Select(task => task.PercentComplete).Average();
				return 0;
			}
		}

		public bool IsActive {
			get {
				return Status.IsAny(JobStatus.Pending, JobStatus.Queued, JobStatus.Scheduled, JobStatus.Processing, JobStatus.Canceling);
			}
		}

		public bool IsOpen {
			get {
				return Status.IsNotAny(JobStatus.Archived);
			}
		}

		public bool CanArchive {
			get {
				return Status.IsAny(JobStatus.Canceled, JobStatus.Faulted, JobStatus.Finished);
			}
		}

		public bool CanCancel {
			get {
				return Status.IsAny(JobStatus.Pending, JobStatus.Queued, JobStatus.Scheduled, JobStatus.Processing);
			}
		}
	}
}