using Orchard.ContentManagement.Aspects;

namespace Orchard.Core.Routable.ViewModels {
#if REFACTORING
   public class RoutableDisplayViewModel  {
        public ContentItemViewModel<IRoutableAspect> Routable {get;set;}
    }
#endif
}