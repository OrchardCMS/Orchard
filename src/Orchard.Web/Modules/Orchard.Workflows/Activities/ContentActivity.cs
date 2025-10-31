using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Activities
{
    public abstract class ContentActivity : Event
    {

        public Localizer T { get; set; }

        public override bool CanStartWorkflow => true;

        public override bool CanExecute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            try
            {

                var contentTypesState = activityContext.GetState<string>("ContentTypes");

                // "" means 'any'
                if (string.IsNullOrEmpty(contentTypesState))
                {
                    return true;
                }

                string[] contentTypes = contentTypesState.Split(',');

                var content = workflowContext.Content;

                if (content == null)
                {
                    return false;
                }

                return contentTypes.Any(contentType => content.ContentItem.TypeDefinition.Name == contentType);
            }
            catch
            {
                return false;
            }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            return new[] { T("Done") };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext)
        {
            yield return T("Done");
        }

        public override string Form => "SelectContentTypes";

        public override LocalizedString Category => T("Content Items");
    }

    public class ContentCreatedActivity : ContentActivity
    {
        public override string Name => "ContentCreated";

        public override LocalizedString Description => T("Content is created.");
    }

    public class ContentUpdatedActivity : ContentActivity
    {
        public override string Name => "ContentUpdated";

        public override LocalizedString Description => T("Content is updated.");
    }

    public class ContentFirstUpdatedActivity : ContentActivity
    {
        public override string Name => "ContentFirstUpdated";

        public override LocalizedString Description => T("Content is updated for the first time.");
    }

    public class ContentPublishedActivity : ContentActivity
    {
        public override string Name => "ContentPublished";


        public override LocalizedString Description => T("Content is published.");
    }

    public class ContentUnpublishedActivity : ContentActivity
    {
        public override string Name => "ContentUnpublished";


        public override LocalizedString Description => T("Content is unpublished.");
    }
    public class ContentVersionedActivity : ContentActivity
    {
        public override string Name => "ContentVersioned";


        public override LocalizedString Description => T("Content is versioned.");
    }

    public class ContentRemovedActivity : ContentActivity
    {
        public override string Name => "ContentRemoved";

        public override LocalizedString Description => T("Content is removed.");
    }
}