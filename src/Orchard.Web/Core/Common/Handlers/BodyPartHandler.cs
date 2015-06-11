using JetBrains.Annotations;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Common.Handlers {
    [UsedImplicitly]
    public class BodyPartHandler : ContentHandler {       
        public BodyPartHandler(IRepository<BodyPartRecord> bodyRepository) {
            Filters.Add(StorageFilter.For(bodyRepository));

            OnIndexing<BodyPart>((context, bodyPart) => context.DocumentIndex
                                                                .Add("body", bodyPart.Record.Text).RemoveTags().Analyze()
                                                                .Add("format", bodyPart.Record.Format).Store());
        }
    }
}