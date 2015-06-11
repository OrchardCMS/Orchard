using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;
using Orchard.Roles.Services;

namespace Orchard.Roles.Forms {
    public class SelectRolesForms : IFormProvider {
        private readonly IRoleService _roleService;
        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public SelectRolesForms(
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
                            )
                        );

                    f._Parts.Add(new SelectListItem { Value = "", Text = T("Any").Text });

                    foreach (var role in _roleService.GetRoles().OrderBy(x => x.Name)) {
                        f._Parts.Add(new SelectListItem { Value = role.Name, Text = role.Name });
                    }

                    return f;
                };

            context.Form("SelectRoles", form);

        }
    }
}