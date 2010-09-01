using Orchard.ContentManagement.Aspects;
using Orchard.Mvc.ViewModels;

namespace Orchard.Core.Routable.ViewModels {
   public class RoutableDisplayViewModel  {
        public ContentItemViewModel<IRoutableAspect> Routable {get;set;}
    }
}