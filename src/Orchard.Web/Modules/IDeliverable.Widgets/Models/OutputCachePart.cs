using Orchard.ContentManagement;

namespace IDeliverable.Widgets.Models
{
    public class OutputCachePart : ContentPart
    {
        public const string GenericSignalName = "OutputCachePartSignal";

        public bool Enabled
        {
            get { return this.Retrieve(x => x.Enabled); }
            set { this.Store(x => x.Enabled, value); }
        }

        public static string ContentSignalName(int id)
        {
            return "OutputCachePartSignal-" + id;
        }

        public static string TypeSignalName(string contentTypeName)
        {
            return "OutputCachePartSignal-" + contentTypeName;
        }
    }
}