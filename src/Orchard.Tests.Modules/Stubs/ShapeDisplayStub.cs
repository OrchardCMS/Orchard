using System.Collections.Generic;
using Orchard.DisplayManagement;

namespace Orchard.Tests.Modules.Stubs
{
    public class ShapeDisplayStub : IShapeDisplay
    {

        public int Calls { get; set; }

        public string Display(Orchard.DisplayManagement.Shapes.Shape shape)
        {
            Calls++;
            return "";
        }

        public string Display(object shape)
        {
            Calls++;
            return "";
        }

        public IEnumerable<string> Display(IEnumerable<object> shapes)
        {
            foreach (var shape in shapes)
            {
                yield return Display(shape);
            }
        }
    }
}
