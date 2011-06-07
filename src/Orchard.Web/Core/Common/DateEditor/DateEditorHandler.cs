using JetBrains.Annotations;
using Orchard.Core.Common.Models;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Common.DateEditor {
    [UsedImplicitly]
    public class DateEditorHandler : ContentHandler {
        public DateEditorHandler() {
            OnPublished<CommonPart>((context, part) => {
                var settings = part.TypePartDefinition.Settings.GetModel<DateEditorSettings>();
                if (!settings.ShowDateEditor) {
                    return;
                }

                var thisIsTheInitialVersionRecord = part.ContentItem.Version < 2;
                var theDatesHaveNotBeenModified = part.CreatedUtc == part.VersionCreatedUtc;
                var theContentDateShouldBeUpdated = thisIsTheInitialVersionRecord && theDatesHaveNotBeenModified;

                if (theContentDateShouldBeUpdated) {
                    // "touch" CreatedUtc in ContentItemRecord
                    part.CreatedUtc = part.PublishedUtc;
                }
            });
        }
    }
}
