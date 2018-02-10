using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement.Utilities;
using Orchard.Core.Containers.Services;
using Orchard.Core.Containers.Settings;
using Orchard.UI.Navigation;

namespace Orchard.Core.Containers.Models {
    public class ContainerPart : ContentPart<ContainerPartRecord> {
        // ReSharper disable InconsistentNaming
        internal LazyField<IEnumerable<ContentTypeDefinition>> ItemContentTypesField = new LazyField<IEnumerable<ContentTypeDefinition>>();
        internal LazyField<ContainerTypePartSettings> ContainerSettingsField = new LazyField<ContainerTypePartSettings>();
        internal LazyField<IListViewProvider> AdminListViewField = new LazyField<IListViewProvider>();
        // ReSharper restore InconsistentNaming

        public IEnumerable<ContentTypeDefinition> ItemContentTypes {
            get { return ItemContentTypesField.Value; }
            set { ItemContentTypesField.Value = value; }
        }

        public ContainerTypePartSettings ContainerSettings {
            get { return ContainerSettingsField.Value; }
        }

        public bool EnablePositioning {
            get { return ContainerSettings.EnablePositioning != null ? ContainerSettings.EnablePositioning.Value : Record.EnablePositioning; }
        }

        public IListViewProvider AdminListView {
            get { return AdminListViewField.Value; }
        }

        public bool ItemsShown {
            get { return Record.ItemsShown; }
            set { Record.ItemsShown = value; }
        }

        public bool Paginated {
            get { return Record.Paginated; }
            set { Record.Paginated = value; }
        }

        public int PageSize {
            get { return Record.PageSize; }
            set { Record.PageSize = value; }
        }

        public bool ShowOnAdminMenu {
            get { return Record.ShowOnAdminMenu; }
            set { Record.ShowOnAdminMenu = value; }
        }

        public string AdminMenuText {
            get { return Record.AdminMenuText; }
            set { Record.AdminMenuText = value; }
        }

        public string AdminMenuPosition {
            get { return Record.AdminMenuPosition; }
            set { Record.AdminMenuPosition = value; }
        }

        public string AdminMenuImageSet {
            get { return Record.AdminMenuImageSet; }
            set { Record.AdminMenuImageSet = value; }
        }

        public int ItemCount {
            get { return Record.ItemCount; }
            set { Record.ItemCount = value; }
        }
    }

    public class ContainerPartRecord : ContentPartRecord {
        public virtual string ItemContentTypes { get; set; }
        public virtual bool ItemsShown { get; set; }
        public virtual bool Paginated { get; set; }
        public virtual int PageSize { get; set; }
        public virtual bool ShowOnAdminMenu { get; set; }
        public virtual string AdminMenuText { get; set; }
        public virtual string AdminMenuPosition { get; set; }
        public virtual string AdminMenuImageSet { get; set; }
        public virtual bool EnablePositioning { get; set; }
        public virtual string AdminListViewName { get; set; }
        public virtual int ItemCount { get; set; }
    }
}
