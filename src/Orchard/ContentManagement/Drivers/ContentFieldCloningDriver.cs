using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard.ContentManagement.Handlers;
using Orchard.Logging;

namespace Orchard.ContentManagement.Drivers {
    public abstract class ContentFieldCloningDriver<TField> : ContentFieldDriver<TField>, IContentFieldCloningDriver where TField : ContentField, new() {

        void IContentFieldCloningDriver.Cloning(CloneContentContext context) {
            ProcessClone(context.ContentItem, context.CloneContentItem, (part, originalField, cloneField) => Cloning(part, originalField, cloneField, context), context.Logger);
        }

        void IContentFieldCloningDriver.Cloned(CloneContentContext context) {
            ProcessClone(context.ContentItem, context.CloneContentItem, (part, originalField, cloneField) => Cloned(part, originalField, cloneField, context), context.Logger);
        }

        void ProcessClone(ContentItem originalItem, ContentItem cloneItem, Action<ContentPart, TField, TField> effort, ILogger logger) {
            var occurences = originalItem.Parts.SelectMany(part => part.Fields.OfType<TField>().Select(field => new { part, field }))
                .Join(cloneItem.Parts.SelectMany(part => part.Fields.OfType<TField>()), original => original.field.Name, cloneField => cloneField.Name, (original, cloneField) => new { original, cloneField });
            occurences.Invoke(pf => effort(pf.original.part, pf.original.field, pf.cloneField), logger);
        }


        protected virtual void Cloning(ContentPart part, TField originalField, TField cloneField, CloneContentContext context) {  }
        protected virtual void Cloned(ContentPart part, TField originalField, TField cloneField, CloneContentContext context) { }
    }
}
