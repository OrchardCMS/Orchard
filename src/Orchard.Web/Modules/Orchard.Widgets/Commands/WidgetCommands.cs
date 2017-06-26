using Orchard.Commands;
using Orchard.Widgets.Services;

namespace Orchard.Widgets.Commands {
    public class WidgetCommands : DefaultOrchardCommandHandler {
        private readonly IWidgetCommandsService _widgetCommandsService;

        public WidgetCommands(
            IWidgetCommandsService widgetCommandsService) {
            _widgetCommandsService = widgetCommandsService;            

            RenderTitle = true;
        }

        [OrchardSwitch]
        public string Title { get; set; }

        [OrchardSwitch]
        public string Name { get; set; }

        [OrchardSwitch]
        public bool RenderTitle { get; set; }

        [OrchardSwitch]
        public string Zone { get; set; }

        [OrchardSwitch]
        public string Position { get; set; }

        [OrchardSwitch]
        public string Layer { get; set; }

        [OrchardSwitch]
        public string Identity { get; set; }

        [OrchardSwitch]
        public string Owner { get; set; }

        [OrchardSwitch]
        public string Text { get; set; }

        [OrchardSwitch]
        public bool UseLoremIpsumText { get; set; }

        [OrchardSwitch]
        public bool Publish { get; set; }

        [OrchardSwitch]
        public string MenuName { get; set; }

        [CommandName("widget create")]
        [CommandHelp("widget create <type> /Title:<title> /Name:<name> /Zone:<zone> /Position:<position> /Layer:<layer> [/Identity:<identity>] [/RenderTitle:true|false] [/Owner:<owner>] [/Text:<text>] [/UseLoremIpsumText:true|false] [/MenuName:<name>]\r\n\t" + "Creates a new widget")]
        [OrchardSwitches("Title,Name,Zone,Position,Layer,Identity,Owner,Text,UseLoremIpsumText,MenuName,RenderTitle")]
        public void Create(string type) {
            var widget = _widgetCommandsService.CreateBaseWidget(
                Context, type, Title, Name, Zone, Position, Layer, Identity, RenderTitle, Owner, Text, UseLoremIpsumText, MenuName);

            if (widget == null) {
                return;
            }

            _widgetCommandsService.Publish(widget);
            Context.Output.WriteLine(T("Widget created successfully.").Text);
        }
    }
}