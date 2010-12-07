using System.Web.Mvc;
using System.Xml;
using Orchard.DisplayManagement.Shapes;
using Orchard.Mvc.Filters;

namespace Orchard.Experimental {
    public class DebugFilter : FilterProvider, IActionFilter {
        public void OnActionExecuting(ActionExecutingContext filterContext) {
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) {
            var viewResultBase = filterContext.Result as ViewResultBase;
            var debugValueResult = filterContext.Controller.ValueProvider.GetValue("$debug");
            if (debugValueResult == null)
                return;

            var debugValue = (string)debugValueResult.ConvertTo(typeof(string));
            if (debugValue == "model" && viewResultBase != null) {
                filterContext.Result = new DebugModelResult(viewResultBase);
            }
        }

        public class DebugModelResult : ActionResult {
            private readonly ViewResultBase _viewResultBase;

            public DebugModelResult(ViewResultBase viewResultBase) {
                _viewResultBase = viewResultBase;
            }

            public override void ExecuteResult(ControllerContext context) {
                context.HttpContext.Response.ContentType = "application/xml";
                var output = context.HttpContext.Response.Output;
                using (var writer = XmlWriter.Create(output, new XmlWriterSettings { Indent = true, IndentChars = "  " })) {
                    try {
                        Writer = writer;
                        var model = _viewResultBase.ViewData.Model;
                        Accept(model);
                    } finally {
                        Writer = null;
                    }
                }
            }

            protected XmlWriter Writer { get; set; }

            void Accept(dynamic model) {
                Shape shape = model;
                Visit(shape);
            }

            void Visit(Shape shape) {
                Writer.WriteStartElement("Shape");
                Writer.WriteAttributeString("Type", shape.Metadata.Type);
                foreach (var item in shape.Items) {
                    Accept(item);
                }
                Writer.WriteEndElement();
            }
        }
    }
}