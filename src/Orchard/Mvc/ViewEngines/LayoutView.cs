using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Orchard.Mvc.ViewEngines {
    public class LayoutView : IView {
        private IView[] _views;

        public LayoutView(IEnumerable<IView> views) {
            _views = views.ToArray();
        }

        public void Render(ViewContext viewContext, TextWriter writer) {

            var orchardViewContext = OrchardLayoutContext.From(viewContext);

            for (var index = 0; index != _views.Length; ++index)
            {
                var view = _views[index];
                if (index == _views.Length - 1) {
                    view.Render(viewContext, writer);
                }
                else {
                    //TEMP: to be replaced with an efficient spooling writer
                    var childWriter = new StringWriter();
                    view.Render(viewContext, childWriter);
                    orchardViewContext.BodyContent = childWriter.ToString();
                }
            }
        }
    }
}
