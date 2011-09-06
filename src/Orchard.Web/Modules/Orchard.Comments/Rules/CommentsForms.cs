using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Orchard.Comments.Rules
{
    [OrchardFeature("Orchard.Comments.Rules")]
    public class CommentsForms : IFormProvider
    {
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public CommentsForms(IShapeFactory shapeFactory)
        {
            Shape = shapeFactory;
        }

        public void Describe(DescribeContext context)
        {
            context.Form("ActionCloseComments",
                shape => Shape.Form(
                Id: "ActionCloseComments",
                _Type: Shape.Textbox(
                    Id: "ContentId", Name: "ContentId",
                    Title: T("Content Item Id"),
                    Description: T("Content Item Id."))
                )
            );
        }
    }
}