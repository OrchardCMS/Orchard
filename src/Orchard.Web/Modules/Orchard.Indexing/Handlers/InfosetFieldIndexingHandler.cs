using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.Handlers;
using Orchard.Indexing.Settings;

namespace Orchard.Indexing.Handlers {
    public class InfosetFieldIndexingHandler : ContentHandler {

        public InfosetFieldIndexingHandler() {

            OnIndexing<InfosetPart>(
                (context, cp) => {
                    var infosetPart = context.ContentItem.As<InfosetPart>();
                    if (infosetPart == null) {
                        return;
                    }

                    // part fields
                    foreach ( var part in infosetPart.ContentItem.Parts ) {
                        foreach ( var field in part.PartDefinition.Fields ) {
                            if (!field.Settings.GetModel<FieldIndexing>().Included) {
                                continue;
                            }

                            var fieldName = field.Name;
                            var value = part.Fields.Where(f => f.Name == fieldName).First().Storage.Get<string>(null);
                            context.DocumentIndex.Add(String.Format("{0}-{1}", infosetPart.TypeDefinition.Name.ToLower(), fieldName.ToLower()), value).RemoveTags().Analyze();
                        }
                    }
                });
        }
    }
}