using IDeliverable.Slides.Helpers;
using IDeliverable.Slides.Services;
using Orchard.ContentManagement;
using Orchard.Localization;

namespace IDeliverable.Slides.SlideshowPlayerEngines.JCarousel
{
    public class JCarousel : SlideshowPlayerEngine
    {
        public override LocalizedString DisplayName => T("JCarousel");

        public bool AutoStart
        {
            get { return this.Retrieve(x => x.AutoStart, () => true); }
            set { this.Store(x => x.AutoStart, value); }
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

        public bool Pagination
        {
            get { return this.Retrieve(x => x.Pagination, () => true); }
            set { this.Store(x => x.Pagination, value); }
        }

        public bool Transitions
        {
            get { return this.Retrieve(x => x.Transitions, () => true); }
            set { this.Store(x => x.Transitions, value); }
        }

        public string Easing
        {
            get { return this.Retrieve(x => x.Easing, () => "linear"); }
            set { this.Store(x => x.Easing, value); }
        }

        public string Wrap
        {
            get { return this.Retrieve(x => x.Wrap, () => "circular"); }
            set { this.Store(x => x.Wrap, value); }
        }

        public bool Vertical
        {
            get { return SlideshowPlayerEngineDataHelper.Retrieve(this, x => x.Vertical); }
            set { this.Store(x => x.Vertical, value); }
        }

        public bool Center
        {
            get { return SlideshowPlayerEngineDataHelper.Retrieve(this, x => x.Center); }
            set { this.Store(x => x.Center, value); }
        }

        public override dynamic BuildEditor(dynamic shapeFactory)
        {
            return UpdateEditor(shapeFactory, null);
        }

        public override dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater)
        {
            updater?.TryUpdateModel(this, Prefix, null, null);

            return shapeFactory.EditorTemplate(TemplateName: "SlideshowPlayerEngines.JCarousel", Prefix: Prefix, Model: this);
        }

        public override dynamic BuildDisplay(dynamic shapeFactory)
        {
            return shapeFactory.SlideshowPlayerEngines_JCarousel();
        }
    }
}