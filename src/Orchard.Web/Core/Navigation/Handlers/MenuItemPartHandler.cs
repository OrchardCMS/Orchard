using Orchard.ContentManagement.Handlers;
using Orchard.Core.Navigation.Models;

namespace Orchard.Core.Navigation.Handlers
{
    public class MenuItemPartHandler : ContentHandler
    {
        public MenuItemPartHandler()
        {
            Filters.Add(new ActivatingFilter<MenuItemPart>("MenuItem"));
        }
    }
}