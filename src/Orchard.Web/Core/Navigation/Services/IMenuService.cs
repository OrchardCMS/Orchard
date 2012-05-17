using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Models;

namespace Orchard.Core.Navigation.Services {
    public interface IMenuService : IDependency {
        IEnumerable<MenuPart> Get();
        IEnumerable<MenuPart> GetMenu(int menuId);
        IContent GetMenu(string name);
        MenuPart Get(int menuPartId);
        IContent Create(string name);
        void Delete(MenuPart menuPart);
    }
}