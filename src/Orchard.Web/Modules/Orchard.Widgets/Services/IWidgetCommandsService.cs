using Orchard.Commands;
using Orchard.Widgets.Models;

namespace Orchard.Widgets.Services
{
    public interface IWidgetCommandsService : IDependency
    {
        WidgetPart CreateBaseWidget(CommandContext context, string type, string title, string name, string zone, string position, string layer, string identity, bool renderTitle, string owner, string text, bool useLoremIpsumText, string menuName);
        void Publish(WidgetPart widget);
    }
}