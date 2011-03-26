using System;

namespace Orchard.ContentManagement {
    public class ContentPartBehavior : ContentItemBehavior {
        private readonly ContentPart _contentPart;

        public ContentPartBehavior(ContentPart contentPart)
            : base(contentPart) {
            _contentPart = contentPart;
        }

        public override object GetMemberMissing(Func<object> proceed, object self, string name) {
            foreach (var field in _contentPart.Fields) {
                if (field.PartFieldDefinition.Name == name)
                    return field;
            }
            return base.GetMemberMissing(proceed, self, name);
        }
    }
}