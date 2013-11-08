using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Roles.Services;

namespace Orchard.Roles.Forms {
    [OrchardFeature("Orchard.Roles.Workflows")]
    public class UserTaskForms : IFormProvider {
        private readonly IRoleService _roleService;
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public UserTaskForms(
            IShapeFactory shapeFactory,
            IRoleService roleService) {
            _roleService = roleService;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, dynamic> form =
                shape => {

                    var f = Shape.Form(
                        Id: "AnyOfRoles",
                        _Parts: Shape.SelectList(
                            Id: "role", Name: "Roles",
                            Title: T("Roles"),
                            Description: T("Select some roles."),
                            Size: 10,
                            Multiple: true
                            ),
                        _Message: Shape.Textbox(
                            Id: "actions", Name: "Actions",
                            Title: T("Available actions."),
                            Description: T("A comma separated list of actions."),
                            Classes: new[] { "text medium" })
                        );

                    f._Parts.Add(new SelectListItem { Value = "", Text = T("Any").Text });

                    foreach (var role in _roleService.GetRoles().OrderBy(x => x.Name)) {
                        f._Parts.Add(new SelectListItem { Value = role.Name, Text = role.Name });
                    }

                    return f;
                };

            context.Form("ActivityUserTask", form);

        }
    }
}