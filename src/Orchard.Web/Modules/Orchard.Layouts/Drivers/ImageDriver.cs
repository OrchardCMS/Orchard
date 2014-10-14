using System.Linq;
using Orchard.ContentManagement;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.ViewModels;
using Orchard.MediaLibrary.Models;
using ContentItem = Orchard.Layouts.Elements.ContentItem;

namespace Orchard.Layouts.Drivers {
    public class ImageDriver : ElementDriver<Image> {
        private readonly IContentManager _contentManager;

        public ImageDriver(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        protected override EditorResult OnBuildEditor(Image element, ElementEditorContext context) {

            var viewModel = new ImageEditorViewModel();
            var editor = context.ShapeFactory.EditorTemplate(TemplateName: "Elements.Image", Model: viewModel);

            if (context.Updater != null) {
                context.Updater.TryUpdateModel(viewModel, context.Prefix, null, null);
                element.ImageId = ParseImageId(viewModel.ImageId);
            }

            var imageId = element.ImageId;
            viewModel.CurrentImage = imageId != null ? GetImage(imageId.Value) : default(ImagePart);

            return Editor(context, editor);
        }

        protected override void OnDisplaying(Image element, ElementDisplayContext context) {
            var imageId = element.ImageId;
            var image = imageId != null ? GetImage(imageId.Value) : default(ImagePart);
            context.ElementShape.ImagePart = image;
        }

        protected ImagePart GetImage(int id) {
            return _contentManager.Get<ImagePart>(id, VersionOptions.Published);
        }

        private static int? ParseImageId(string imageId) {
            return ContentItem.Deserialize(imageId).FirstOrDefault();
        }
    }
}