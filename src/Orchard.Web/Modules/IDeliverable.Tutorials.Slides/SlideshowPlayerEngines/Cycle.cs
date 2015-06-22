using IDeliverable.Slides.Helpers;
using IDeliverable.Slides.Services;
using IUpdateModel = Orchard.ContentManagement.IUpdateModel;

namespace IDeliverable.Tutorials.Slides.SlideshowPlayerEngines
{
    public class Cycle : SlideshowPlayerEngine
    {
        public int Speed
        {
            get { return this.Retrieve(x => x.Speed, () => 500); }
            set { this.Store(x => x.Speed, value); }
        }

        public int? ManualSpeed
        {
            get { return this.Retrieve(x => x.ManualSpeed); }
            set { this.Store(x => x.ManualSpeed, value); }
        }

        public int Timeout
        {
            get { return this.Retrieve(x => x.Timeout, () => 4000); }
            set { this.Store(x => x.Timeout, value); }
        }

        public bool AllowWrap
        {
            get { return this.Retrieve(x => x.AllowWrap, () => true); }
            set { this.Store(x => x.AllowWrap, value); }
        }

        public string AutoHeight
        {
            get { return this.Retrieve(x => x.AutoHeight, () => "0"); }
            set { this.Store(x => x.AutoHeight, value); }
        }

        public int Loop
        {
            get { return this.Retrieve(x => x.Loop, () => 0); }
            set { this.Store(x => x.Loop, value); }
        }

        public bool Paused
        {
            get { return this.Retrieve(x => x.Paused); }
            set { this.Store(x => x.Paused, value); }
        }

        public bool PauseOnHover
        {
            get { return this.Retrieve(x => x.PauseOnHover); }
            set { this.Store(x => x.PauseOnHover, value); }
        }

        public bool Random
        {
            get { return this.Retrieve(x => x.Random); }
            set { this.Store(x => x.Random, value); }
        }

        public bool Reverse
        {
            get { return this.Retrieve(x => x.Reverse); }
            set { this.Store(x => x.Reverse, value); }
        }

        public override dynamic BuildEditor(dynamic shapeFactory)
        {
            return UpdateEditor(shapeFactory, null);
        }

        public override dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater)
        {
            updater?.TryUpdateModel(this, Prefix, null, null);
            return shapeFactory.EditorTemplate(TemplateName: "SlideshowPlayerEngines.Cycle", Model: this, Prefix: Prefix);
        }

        public override dynamic BuildDisplay(dynamic shapeFactory)
        {
            return shapeFactory.SlideshowPlayerEngines_Cycle();
        }
    }
}