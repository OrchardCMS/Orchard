using System;
using Orchard.DisplayManagement;
using Orchard.Events;
using Orchard.Localization;

namespace Orchard.Comments.Rules {
    public interface IFormProvider : IEventHandler {
        void Describe(dynamic context);
    }

    public class CommentsForms : IFormProvider {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public CommentsForms(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(dynamic context) {

            Func<IShapeFactory, dynamic> form =
                shape => Shape.Form(
                    Id: "ActionCloseComments",
                    _Type: Shape.Textbox(
                        Id: "ContentId", Name: "ContentId",
                        Title: T("Content Item Id"),
                        Description: T("Content Item Id."),
                        Classes: new [] { "tokenized" })
                );

            context.Form("ActionCloseComments", form);
        }
    }
}