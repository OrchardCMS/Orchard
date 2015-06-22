using IDeliverable.Slides.Helpers;
using IDeliverable.Slides.Services;
using Orchard.ContentManagement;

namespace IDeliverable.Slides.SlideshowPlayerEngines.Bootstrap
{
    public class Bootstrap : SlideshowPlayerEngine
    {
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
            get { return SlideshowPlayerEngineDataHelper.Retrieve(this, x => x.Wrap); }
            set { this.Store(x => x.Wrap, value); }
        }

        public bool Keyboard
        {
            get { return SlideshowPlayerEngineDataHelper.Retrieve(this, x => x.Keyboard); }
            set { this.Store(x => x.Keyboard, value); }
        }

        public override dynamic BuildEditor(dynamic shapeFactory)
        {
            return UpdateEditor(shapeFactory, null);
        }

        public override dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater)
        {
            updater?.TryUpdateModel(this, Prefix, null, null);

            return shapeFactory.EditorTemplate(TemplateName: "SlideshowPlayerEngines.Bootstrap", Prefix: Prefix, Model: this);
        }

        public override dynamic BuildDisplay(dynamic shapeFactory)
        {
            return shapeFactory.SlideshowPlayerEngines_Bootstrap();
        }
    }
}