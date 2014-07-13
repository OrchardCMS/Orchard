using System.Xml.Linq;
using Orchard.Azure.MediaServices.Models.Records;
using Orchard.Core.Common.Utilities;

namespace Orchard.Azure.MediaServices.Models.Jobs {
    public class Task {
        internal readonly LazyField<Job> _jobField = new LazyField<Job>();

        public Task() {
            Record = new TaskRecord();
        }

        public TaskRecord Record { get; set; }

        public Job Job {
            get { return _jobField.Value; }
            set { _jobField.Value = value; }
        }

        public string WamsTaskId {
            get { return Record.WamsTaskId; }
            set { Record.WamsTaskId = value; }
        }

        public string TaskProviderName {
            get { return Record.TaskProviderName; }
            set { Record.TaskProviderName = value; }
        }

        public int Index {
            get { return Record.TaskIndex; }
            set { Record.TaskIndex = value; }
        }

        public JobStatus Status {
            get { return Record.Status; }
            set { Record.Status = value; }
        }

        public int PercentComplete {
            get { return Record.PercentComplete; }
            set { Record.PercentComplete = value; }
        }

        public XElement Settings {
            get { return Record.SettingsXml != null ? XElement.Parse(Record.SettingsXml) : new XElement("Settings"); }
            set { Record.SettingsXml = value != null ? value.ToString() : null; }
        }

        public string HarvestAssetType {
            get { return Record.HarvestAssetType; }
            set { Record.HarvestAssetType = value; }
        }

        public string HarvestAssetName {
            get { return Record.HarvestAssetName; }
            set { Record.HarvestAssetName = value; }
        }
    }
}