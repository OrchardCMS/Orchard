using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.ContentTypes.Events;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Layout;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.Projections.ViewModels;
using Orchard.UI.Notify;

namespace Orchard.Projections.Settings {
    public class ProjectionPartEditorEvents : ContentDefinitionEditorEventsBase, IContentDefinitionEventHandler {
        private readonly IProjectionManager _projectionManager;
        private readonly IContentManager _contentManager;
        private readonly IRepository<LayoutRecord> _layoutRepository;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ProjectionPartEditorEvents(
            IOrchardServices services,
            IProjectionManager projectionManager,
            IContentManager contentManager,
            IRepository<LayoutRecord> layoutRepository,
            IContentDefinitionManager contentDefinitionManager) {

            _projectionManager = projectionManager;
            _contentManager = contentManager;
            _layoutRepository = layoutRepository;
            _contentDefinitionManager = contentDefinitionManager;
            Services = services;
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }

        public IOrchardServices Services { get; set; }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name == "ProjectionPart") {
                var model = definition.Settings.GetModel<ProjectionPartSettings>();
                model.QueryRecordEntries = GetQueriesRecordEntry();
                if (!string.IsNullOrWhiteSpace(model.FilterQueryRecordId)) {
                    model.FilterQueryRecordsId = model.FilterQueryRecordId.Split('&');
                }
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "ProjectionPart") {
                yield break;
            }

            var model = new ProjectionPartSettings();
            model.QueryRecordEntries = GetQueriesRecordEntry();


            if(updateModel.TryUpdateModel(model, "ProjectionPartSettings", null, null)) { 
                if (model.FilterQueryRecordsId != null && model.FilterQueryRecordsId.Count()>0) {
                    // check if default query selected is in filter queries list
                    if (!model.FilterQueryRecordsId.Contains(model.QueryLayoutRecordId) && model.QueryLayoutRecordId!="-1") {
                        updateModel.AddModelError("ProjectionPart", T("The default query must be one of the selected queries"));
                    }

                    // also save the identity part of the query and guid of the layout to be used in the import
                    model.IdentityQueryLayoutRecord = GetIdentityQueryLayout(model.QueryLayoutRecordId);

                    model.FilterQueryRecordId = string.Join("&", model.FilterQueryRecordsId);

                    List<string> identityForFilterQuery = new List<string>();
                    foreach (var record in model.FilterQueryRecordsId) {
                        identityForFilterQuery.Add(GetIdentityQueryLayout(record));
                    }
                    model.IdentityFilterQueryRecord = string.Join("&", identityForFilterQuery);
                }

                builder
                    .WithSetting("ProjectionPartSettings.QueryLayoutRecordId", model.QueryLayoutRecordId)
                    .WithSetting("ProjectionPartSettings.IdentityQueryLayoutRecord", model.IdentityQueryLayoutRecord)
                    .WithSetting("ProjectionPartSettings.FilterQueryRecordId", model.FilterQueryRecordId)
                    .WithSetting("ProjectionPartSettings.IdentityFilterQueryRecord", model.IdentityFilterQueryRecord)
                    .WithSetting("ProjectionPartSettings.Items", model.Items.ToString())
                    .WithSetting("ProjectionPartSettings.LockEditingItems", model.LockEditingItems.ToString())
                    .WithSetting("ProjectionPartSettings.Skip", model.Skip.ToString())
                    .WithSetting("ProjectionPartSettings.LockEditingSkip", model.LockEditingSkip.ToString())
                    .WithSetting("ProjectionPartSettings.MaxItems", model.MaxItems.ToString())
                    .WithSetting("ProjectionPartSettings.LockEditingMaxItems", model.LockEditingMaxItems.ToString())
                    .WithSetting("ProjectionPartSettings.PagerSuffix", model.PagerSuffix)
                    .WithSetting("ProjectionPartSettings.LockEditingPagerSuffix", model.LockEditingPagerSuffix.ToString())
                    .WithSetting("ProjectionPartSettings.DisplayPager", model.DisplayPager.ToString())
                    .WithSetting("ProjectionPartSettings.LockEditingDisplayPager", model.LockEditingDisplayPager.ToString());
            }
            yield return DefinitionTemplate(model);
        }

        #region Implementation interface
        public void ContentFieldAttached(ContentFieldAttachedContext context) {
        }

        public void ContentFieldDetached(ContentFieldDetachedContext context) {
        }

        public void ContentPartAttached(ContentPartAttachedContext context) {
        }

        public void ContentPartCreated(ContentPartCreatedContext context) {
        }

        public void ContentPartDetached(ContentPartDetachedContext context) {
        }

        public void ContentPartImported(ContentPartImportedContext context) {
        }

        public void ContentPartImporting(ContentPartImportingContext context) {
        }

        public void ContentPartRemoved(ContentPartRemovedContext context) {
        }

        public void ContentTypeCreated(ContentTypeCreatedContext context) {
        }

        public void ContentTypeImported(ContentTypeImportedContext context) {
            var part = context.ContentTypeDefinition.Parts
                .ToList()
                .Where(p => p.PartDefinition.Name == "ProjectionPart")
                .FirstOrDefault();
            if (part != null) {
                var settings = part.Settings.GetModel<ProjectionPartSettings>();

                // from identity part of the query and guid of the layout find reference
                settings.QueryLayoutRecordId = string.IsNullOrWhiteSpace(settings.IdentityQueryLayoutRecord)
                    ? "-1" : GetQueryLayoutRecord(settings.IdentityQueryLayoutRecord);
                
                if (!string.IsNullOrWhiteSpace(settings.IdentityFilterQueryRecord)) {
                    List<string> identityForFilterQuery = new List<string>();
                    foreach (var record in settings.IdentityFilterQueryRecord.Split('&').ToList()) {
                        var correctId = GetQueryLayoutRecord(record);
                        if (!string.IsNullOrEmpty(correctId)) {
                            identityForFilterQuery.Add(correctId);
                        }
                    }
                    settings.FilterQueryRecordId = string.Join("&", identityForFilterQuery);
                } else {
                    settings.FilterQueryRecordId = string.Empty;
                }
                
                _contentDefinitionManager.AlterTypeDefinition(context.ContentTypeDefinition.Name, cfg => cfg
                     .WithPart(part.PartDefinition.Name,
                          pb => pb
                            .WithSetting("ProjectionPartSettings.QueryLayoutRecordId", settings.QueryLayoutRecordId)
                            .WithSetting("ProjectionPartSettings.FilterQueryRecordId", settings.FilterQueryRecordId))
                  );
            }
        }

        public void ContentTypeImporting(ContentTypeImportingContext context) {
        }

        public void ContentTypeRemoved(ContentTypeRemovedContext context) {
        }
        #endregion

        private string GetQueryLayoutRecord(string record) {
            var ids = record.Split(';');
            if (ids.Count() == 1) {
                // if is present only -1, the default query has not been selected
                return ids[0];
            }
            else {
                string stringIds = string.Empty;

                var ciIdentity = _contentManager.ResolveIdentity(new ContentIdentity(ids[0]));
                if (ciIdentity != null) {
                    stringIds = ciIdentity.Id.ToString() + ";";

                    if (ids[1] == "-1") {
                        // default layout
                        stringIds += "-1";
                    }
                    else {
                        var recordLayout = _layoutRepository.Fetch(l => l.GUIdentifier == ids[1]).FirstOrDefault();
                        if (recordLayout != null) {
                            stringIds += recordLayout.Id.ToString();
                        }
                    }
                }
                return stringIds;
            }
        }
        private string GetIdentityQueryLayout(string record) {

            var ids = record.Split(';');
            if (ids.Count() == 1) {
                // if is present only -1, the default query has not been selected
                return ids[0];
            }
            else { 
                // ids[0] is id of query
                // ids[1] is record id of layout
                var identityQueryLayout = string.Empty;
                // identity part to identify the query
                var content = _contentManager.Get(int.Parse(ids[0]));
                if (content != null) {
                    var identity = _contentManager.GetItemMetadata(content).Identity;
                    if (identity != null) {
                        identityQueryLayout = identity.ToString() + ";";
                    }
                    else {
                        Services.Notifier.Error(T("ProjectionPart - Query - The loaded id {0} does not exist", ids[0]));
                    }
                }
                // guid id to identify the layout
                if (ids[1] == "-1") {
                    // default layout
                    identityQueryLayout += ids[1];
                }
                else {
                    var layoutRecord = _layoutRepository.Get(int.Parse(ids[1]));
                    if (layoutRecord != null) {
                        identityQueryLayout += layoutRecord.GUIdentifier;
                    }
                    else {
                        Services.Notifier.Error(T("ProjectionPart - Layout of query - The loaded id {0} does not exist", ids[1]));
                    }
                }
                return identityQueryLayout;
            }
        }

        private IEnumerable<QueryRecordEntry> GetQueriesRecordEntry() {
            // populating the list of queries and layouts
            var layouts = _projectionManager.DescribeLayouts().SelectMany(x => x.Descriptors).ToList();

            List<QueryRecordEntry> records = new List<QueryRecordEntry>();
            records.Add(new QueryRecordEntry {
                Id = -1,
                Name = T("No default").Text,
                LayoutRecordEntries = new List<LayoutRecordEntry>()
            });

            records.AddRange(Services.ContentManager.Query<QueryPart, QueryPartRecord>().Join<TitlePartRecord>().OrderBy(x => x.Title).List()
                .Select(x => new QueryRecordEntry {
                    Id = x.Id,
                    Name = x.Name,
                    LayoutRecordEntries = x.Layouts
                        .Select(l => new LayoutRecordEntry {
                            Id = l.Id,
                            Description = GetLayoutDescription(layouts, l)
                        })
                        .ToList()
                }));
            return records;
        }

        private string GetLayoutDescription(IEnumerable<LayoutDescriptor> layouts, LayoutRecord l) {
            var descriptor = layouts.FirstOrDefault(x => l.Category == x.Category && l.Type == x.Type);
            return String.IsNullOrWhiteSpace(l.Description) ? descriptor.Display(new LayoutContext { State = FormParametersHelper.ToDynamic(l.State) }).Text : l.Description;
        }
    }
}
