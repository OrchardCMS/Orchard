using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Core.Title.Models;
using Orchard.Data.Conventions;
using Orchard.Environment.Extensions;

namespace Orchard.ImportExport.Models {
    [OrchardFeature("Orchard.Deployment")]
    public class DeploymentSubscriptionPartRecord : ContentPartRecord {
        public virtual int? DeploymentConfigurationId { get; set; }
        public virtual bool IncludeMetadata { get; set; }
        public virtual bool IncludeData { get; set; }
        public virtual bool IncludeFiles { get; set; }
        public virtual bool DeployAsDrafts { get; set; }
        public virtual VersionHistoryOptions VersionHistoryOption { get; set; }
        public virtual FilterOptions Filter { get; set; }
        public virtual string DeploymentType { get; set; }

        [StringLengthMax]
        public virtual string ContentTypes { get; set; }

        public virtual string QueryIdentity { get; set; }
        public virtual DateTime? DeployedChangesToUtc { get; set; }
    }

    public class DeploymentSubscriptionPart : ContentPart<DeploymentSubscriptionPartRecord> {
        public string Title {
            get { return ContentItem.As<TitlePart>().Title; }
            set { ContentItem.As<TitlePart>().Title = value; }
        }

        public IContent DeploymentConfiguration {
            get {
                return Record.DeploymentConfigurationId.HasValue ?
                    ContentItem.ContentManager.Get(Record.DeploymentConfigurationId.Value) : null;
            }
            set { Record.DeploymentConfigurationId = value.Id; }
        }

        public DeploymentType DeploymentType {
            get {
                DeploymentType type;
                return Enum.TryParse(Record.DeploymentType, out type) ? type : DeploymentType.Export;
            }
            set { Record.DeploymentType = value.ToString(); }
        }

        public bool IncludeMetadata {
            get { return Record.IncludeMetadata; }
            set { Record.IncludeMetadata = value; }
        }

        public bool IncludeData {
            get { return Record.IncludeData; }
            set { Record.IncludeData = value; }
        }

        public bool IncludeFiles {
            get { return Record.IncludeFiles; }
            set { Record.IncludeFiles = value; }
        }

        public bool DeployAsDrafts {
            get { return Record.DeployAsDrafts; }
            set { Record.DeployAsDrafts = value; }
        }

        public VersionHistoryOptions VersionHistoryOption {
            get { return Record.VersionHistoryOption; }
            set { Record.VersionHistoryOption = value; }
        }

        public FilterOptions Filter {
            get { return Record.Filter; }
            set { Record.Filter = value; }
        }

        public List<string> ContentTypes {
            get {
                return string.IsNullOrEmpty(Record.ContentTypes) ?
                    new List<string>() : Record.ContentTypes.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            set { Record.ContentTypes = string.Join(",", value); }
        }

        public string QueryIdentity {
            get { return Record.QueryIdentity; }
            set { Record.QueryIdentity = value; }
        }

        public DateTime? DeployedChangesToUtc {
            get { return Record.DeployedChangesToUtc; }
            set { Record.DeployedChangesToUtc = value; }
        }
    }

    public enum FilterOptions {
        AllItems,
        ChangesSinceLastImport,
        QueuedDeployableItems
    }
}
