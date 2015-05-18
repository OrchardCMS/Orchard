﻿using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Tasks.Indexing;

namespace Orchard.Indexing.Settings {
    public class EditorEvents : ContentDefinitionEditorEventsBase {
        private readonly IIndexingTaskManager _indexingTaskManager;
        private readonly IContentManager _contentManager;

        private const int PageSize = 50;

        public EditorEvents(IIndexingTaskManager indexingTaskManager, IContentManager contentManager){
            _indexingTaskManager = indexingTaskManager;
            _contentManager = contentManager;
        }

        private string _contentTypeName;
        private bool _tasksCreated;

        public override IEnumerable<TemplateViewModel> TypeEditor(ContentTypeDefinition definition) {
            var model = definition.Settings.GetModel<TypeIndexing>();
            _contentTypeName = definition.Name;
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypeEditorUpdate(ContentTypeDefinitionBuilder builder, IUpdateModel updateModel) {
            var model = new TypeIndexing();
            updateModel.TryUpdateModel(model, "TypeIndexing", null, null);
            builder.WithSetting("TypeIndexing.Indexes", model.Indexes);

            CreateIndexingTasks();
            
            yield return DefinitionTemplate(model);
        }

        /// <summary>
        /// Creates new indexing tasks to update the index document for these content items
        /// </summary>
        private void CreateIndexingTasks()
        {
            if (!_tasksCreated) {
                CreateTasksForType(_contentTypeName);
                _tasksCreated = true;
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            var model = definition.Settings.GetModel<FieldIndexing>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            var model = new FieldIndexing();
            updateModel.TryUpdateModel(model, "FieldIndexing", null, null);
            builder.WithSetting("FieldIndexing.Included", model.Included ? true.ToString() : null);

            CreateIndexingTasks();

            yield return DefinitionTemplate(model);
        }

        private void CreateTasksForType(string type) {
            var index = 0;
            bool contentItemProcessed;

            // todo: load ids only, or create a queued job

            do {
                contentItemProcessed = false;
                var contentItemsToIndex = _contentManager.Query(VersionOptions.Published, new [] { type }).Slice(index, PageSize);

                foreach (var contentItem in contentItemsToIndex) {
                    contentItemProcessed = true;
                    _indexingTaskManager.CreateUpdateIndexTask(contentItem);
                }

                index += PageSize;

            } while (contentItemProcessed);
        }
    }
}