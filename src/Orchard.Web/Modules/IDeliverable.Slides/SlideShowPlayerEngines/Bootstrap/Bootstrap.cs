using IDeliverable.Slides.Helpers;
using IDeliverable.Slides.Services;
using Orchard.ContentManagement;
using Orchard.Localization;

namespace IDeliverable.Slides.SlideShowPlayerEngines.Bootstrap
{
    public class Bootstrap : SlideShowPlayerEngine
    {
        public override LocalizedString DisplayName
        {
            get { return T("Bootstrap"); }
        }

        public string Prefix
        {
            get { return "Bootstrap"; }
        }

        public int Interval
        {
            get { return this.Retrieve(x => x.Interval, () => 3000); }
            set { this.Store(x => x.Interval, value); }
        }

        public bool Controls
        {
            get { return this.Retrieve(x => x.Controls, () => true); }
            set { this.Store(x => x.Controls, value); }
        }

        public bool Indicators
        {
            get { return this.Retrieve(x => x.Indicators, () => true); }
            set { this.Store(x => x.Indicators, value); }
        }

        public string Pause
        {
            get { return this.Retrieve(x => x.Pause, () => "hover"); }
            set { this.Store(x => x.Pause, value); }
        }

        public bool Wrap
        {
            get { return SlideShowPlayerEngineDataHelper.Retrieve(this, x => x.Wrap); }
            set { this.Store(x => x.Wrap, value); }
        }

        public bool Keyboard
        {
            get { return SlideShowPlayerEngineDataHelper.Retrieve(this, x => x.Keyboard); }
            set { this.Store(x => x.Keyboard, value); }
        }

        public override dynamic BuildSettingsEditor(dynamic shapeFactory)
        {
            return UpdateSettingsEditor(shapeFactory, null);
        }

        public override dynamic UpdateSettingsEditor(dynamic shapeFactory, IUpdateModel updater)
        {
            if (updater != null)
            {
                updater.TryUpdateModel(this, Prefix, null, null);
            }

            return shapeFactory.EditorTemplate(TemplateName: "Engines.Bootstrap", Prefix: Prefix, Model: this);
        }

        public override dynamic BuildDisplay(dynamic shapeFactory)
        {
            return shapeFactory.Engines_Bootstrap();
        }
    }
}