using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Core.Title.Models;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Layout;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.Projections.ViewModels;

namespace Orchard.Projections.Settings {
    public class ProjectionPartEditorEvents : ContentDefinitionEditorEventsBase {
        private readonly IProjectionManagerExtension _projectionManager;

        public ProjectionPartEditorEvents(
            IOrchardServices services,
            IProjectionManagerExtension projectionManager) {

            _projectionManager = projectionManager;
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
                    if (!model.FilterQueryRecordsId.Contains(model.QueryLayoutRecordId)) {
                        updateModel.AddModelError("ProjectionPart", T("The default query must be a part of the filtered queries"));
                    }

                    model.FilterQueryRecordId = string.Join("&", model.FilterQueryRecordsId);
                }

                builder
                    .WithSetting("ProjectionPartSettings.QueryLayoutRecordId", model.QueryLayoutRecordId)
                    .WithSetting("ProjectionPartSettings.FilterQueryRecordId", model.FilterQueryRecordId)
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