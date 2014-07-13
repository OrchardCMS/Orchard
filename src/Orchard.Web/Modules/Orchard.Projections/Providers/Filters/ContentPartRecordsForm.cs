using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement.Records;
using Orchard.DisplayManagement;
using Orchard.Environment.ShellBuilders.Models;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Orchard.Projections.Providers.Filters {
    public class ContentPartRecordsForm : IFormProvider {
        private readonly ShellBlueprint _shellBlueprint;
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ContentPartRecordsForm(
            ShellBlueprint shellBlueprint,
            IShapeFactory shapeFactory) {
            _shellBlueprint = shellBlueprint;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form =
                shape => {

                    var f = Shape.Form(
                        Id: "AnyOfContentPartRecords",
                        _Parts: Shape.SelectList(
                            Id: "contentpartrecordss", Name: "ContentPartRecords",
                            Title: T("Content part records"),
                            Description: T("Select some content part records."),
                            Size: 10,
                            Multiple: true
                            )
                        );

                    foreach (var recordBluePrint in _shellBlueprint.Records.OrderBy(x => x.Type.Name)) {
                        if (typeof(ContentPartRecord).IsAssignableFrom(recordBluePrint.Type) || typeof(ContentPartVersionRecord).IsAssignableFrom(recordBluePrint.Type)) {
                            f._Parts.Add(new SelectListItem { Value = recordBluePrint.Type.Name, Text = recordBluePrint.Type.Name });
                        }
                    }

                    return f;
                };

            context.Form("ContentPartRecordsForm", form);

        }
    }
}