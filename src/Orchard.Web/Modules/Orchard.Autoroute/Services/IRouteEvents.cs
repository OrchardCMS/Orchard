using System;
using Orchard.ContentManagement;
using Orchard.Events;

namespace Orchard.Autoroute.Services {
    public interface IRouteEvents : IEventHandler {
        void Routed(IContent content, String path);
    }
}