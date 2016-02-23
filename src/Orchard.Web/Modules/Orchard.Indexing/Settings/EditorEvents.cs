using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Indexing.Services;

namespace Orchard.Indexing.Settings {
    public class EditorEvents : ContentDefinitionEditorEventsBase {
        private readonly IJobsQueueService _jobsQueueService;

        public EditorEvents(IJobsQueueService jobsQueueService) {
            _jobsQueueService = jobsQueueService;
        }

        private string _contentTypeName;
        private bool _tasksCreated;

        public override IEnumerable<TemplateViewModel> TypeEditor(ContentTypeDefinition definition) {
            var model = definition.Settings.GetModel<TypeIndexing>();
            _contentTypeName = definition.Name;
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypeEditorUpdate(ContentTypeDefinitionBuilder builder, IUpdateModel updateModel) {
            var previous = builder.Current.Settings.GetModel<TypeIndexing>();

            var model = new TypeIndexing();
            updateModel.TryUpdateModel(model, "TypeIndexing", null, null);
            builder.WithSetting("TypeIndexing.Indexes", model.Indexes);

            // create indexing tasks only if settings have changed
            if (Clean(model.Indexes) != Clean(previous.Indexes)) {

                // if a an index is added, all existing content items need to be re-indexed
                CreateIndexingTasks();
            }

            yield return DefinitionTemplate(model);
        }

        private string Clean(string value) {
            if (String.IsNullOrEmpty(value))
                return value;

            return value.Trim(',', ' ');
        }

        /// <summary>
        /// Creates new indexing tasks to update the index document for these content items
        /// </summary>
        private void CreateIndexingTasks() {
            if (!_tasksCreated) {
                // Creating tasks with Jobs is needed because editing content type settings for a type with many items causes OutOfMemoryException, see issue: https://github.com/OrchardCMS/Orchard/issues/4729
                _jobsQueueService.Enqueue("ICreateUpdateIndexTaskService.CreateNextIndexingTaskBatch", new Dictionary<string, object> { { "contentTypeName", _contentTypeName }, { "currentBatchIndex", "0" } }, CreateUpdateIndexTaskService.JobPriority);
                _tasksCreated = true;
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            var model = definition.Settings.GetModel<FieldIndexing>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            var previous = builder.Current.Settings.GetModel<FieldIndexing>();

            var model = new FieldIndexing();
            updateModel.TryUpdateModel(model, "FieldIndexing", null, null);
            builder.WithSetting("FieldIndexing.Included", model.Included ? true.ToString() : null);

            // create indexing tasks only if settings have changed
            if (model.Included != previous.Included) {

                // if a field setting has changed, all existing content items need to be re-indexed
                CreateIndexingTasks();
            }

            yield return DefinitionTemplate(model);
        }
    }
}