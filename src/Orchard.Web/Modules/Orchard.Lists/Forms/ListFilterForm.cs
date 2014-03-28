using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Containers.Models;
using Orchard.Core.Containers.Services;
using Orchard.DisplayManagement;
using Orchard.Lists.Services;

namespace Orchard.Lists.Forms {
    public class ListFilterForm : Component, IFormProvider {
        private readonly IContainerService _containerService;
        private readonly IContentManager _contentManager;

        public ListFilterForm(IShapeFactory shapeFactory, IContainerService containerService, IContentManager contentManager) {
            _containerService = containerService;
            _contentManager = contentManager;
            New = shapeFactory;
        }
        protected dynamic New { get; set; }

        public void Describe(dynamic context) {
            Func<IShapeFactory, object> form =
                shape => {
                    var f = New.Form(
                        Id: "List",
                        _Lists: New.SelectList(
                            Id: "listId", Name: "ListId",
                            Title: T("List"),
                            Description: T("Select a list."),
                            Multiple: false));

                    foreach (var list in _containerService.GetContainers(VersionOptions.Latest).OrderBy(GetListName)) {
                        f._Lists.Add(new SelectListItem {Value = list.Id.ToString(CultureInfo.InvariantCulture), Text = GetListName(list)});
                    }

                    return f;
                };

            context.Form("ListFilter", form);
        }

        private string GetListName(ContainerPart containerPart) {
            return _contentManager.GetItemMetadata(containerPart).DisplayText;
        }
    }
}