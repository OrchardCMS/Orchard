using System.Linq;
using Orchard.ContentManagement;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.ViewModels;
using Orchard.MediaLibrary.Models;
using ContentItem = Orchard.Layouts.Elements.ContentItem;

namespace Orchard.Layouts.Drivers {
    public class VectorGraphicDriver : ElementDriver<VectorGraphic> {
        private readonly IContentManager _contentManager;

        public VectorGraphicDriver(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        protected override EditorResult OnBuildEditor(VectorGraphic element, ElementEditorContext context) {

            var viewModel = new VectorGraphicEditorViewModel {
                VectorGraphicId = element.MediaId.ToString(),
                Width = element.Width,
                Height = element.Height
            };
            var editor = context.ShapeFactory.EditorTemplate(TemplateName: "Elements.VectorGraphic", Model: viewModel);

            if (context.Updater != null) {
                context.Updater.TryUpdateModel(viewModel, context.Prefix, null, null);
                element.MediaId = ParseVectorGraphicId(viewModel.VectorGraphicId);
                element.Width = viewModel.Width;
                element.Height = viewModel.Height;
            }

            var mediaId = element.MediaId;
            viewModel.CurrentVectorGraphic = mediaId != null ? GetVectorGraphic(mediaId.Value) : default(VectorGraphicPart);

            return Editor(context, editor);
        }

        protected override void OnDisplaying(VectorGraphic element, ElementDisplayContext context) {
            var mediaId = element.MediaId;
            var vectorGraphic = mediaId != null ? GetVectorGraphic(mediaId.Value) : default(VectorGraphicPart);
            context.ElementShape.VectorGraphicPart = vectorGraphic;
        }

        protected VectorGraphicPart GetVectorGraphic(int id) {
            return _contentManager.Get<VectorGraphicPart>(id, VersionOptions.Published);
        }

        private static int? ParseVectorGraphicId(string vectorGraphicId) {
            return ContentItem.Deserialize(vectorGraphicId).FirstOrDefault();
        }
    }
}