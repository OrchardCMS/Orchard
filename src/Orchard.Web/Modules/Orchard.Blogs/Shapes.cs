//using Orchard.ContentManagement;
//using Orchard.DisplayManagement.Descriptors;

//namespace Orchard.Blogs {
//    public class Shapes : IShapeTableProvider {
//        public void Discover(ShapeTableBuilder builder) {
//            builder.Describe("Items_Content__Blog")
//                .OnCreated(created => {
//                    var blog = created.Shape;
//                    blog.Content.Add(created.New.Parts_Blogs_BlogPost_List(ContentPart: blog.ContentItem, ContentItems: blog.));
//                })
//                .OnDisplaying(displaying => {
//                    ContentItem contentItem = displaying.Shape.ContentItem;
//                    if (contentItem != null) {
//                        var zoneName = contentItem.As<WidgetPart>().Zone;
//                        displaying.ShapeMetadata.Alternates.Add("Items_Widget__" + contentItem.ContentType);
//                        displaying.ShapeMetadata.Alternates.Add("Items_Widget__" + zoneName);
//                    }
//                });
//        }
//    }
//}
