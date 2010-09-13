using Orchard.Core.Routable.Models;

namespace Orchard.Core.Routable.ViewModels {
   public class RoutableDisplayViewModel  {
       public string Title { get { return RoutePart.Title; } }
       public RoutePart RoutePart { get; set; }
   }
}