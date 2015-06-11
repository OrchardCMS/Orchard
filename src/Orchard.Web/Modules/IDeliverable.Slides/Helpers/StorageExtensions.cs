using IDeliverable.Slides.Services;

namespace IDeliverable.Slides.Helpers
{
    public static class StorageExtensions
    {
        public static string RetrieveSlidesData(this IStorage storage)
        {
            return storage.Retrieve<string>("SlideShowSlides");
        }

        public static void StoreSlidesData(this IStorage storage, string value)
        {
            storage.Store("SlideShowSlides", value);
        }

        public static int? RetrieveQueryId(this IStorage storage)
        {
            return storage.Retrieve<int?>("SlideShowQueryId");
        }

        public static void StoreQueryId(this IStorage storage, int? value)
        {
            storage.Store("SlideShowQueryId", value);
        }

        public static string RetrieveProjectionSlidesDisplayType(this IStorage storage) {
            return storage.Retrieve<string>("SlideShowProjectionSlidesDisplayType");
        }

        public static void StoreProjectionSlidesDisplayType(this IStorage storage, string value)
        {
            storage.Store("SlideShowProjectionSlidesDisplayType", value);
        }

        public static int? RetrieveListId(this IStorage storage)
        {
            return storage.Retrieve<int?>("SlideShowListId");
        }

        public static void StoreListId(this IStorage storage, int? value)
        {
            storage.Store("SlideShowListId", value);
        }

        public static string RetrieveListSlidesDisplayType(this IStorage storage)
        {
            return storage.Retrieve<string>("SlideShowListSlidesDisplayType");
        }

        public static void StoreListSlidesDisplayType(this IStorage storage, string value)
        {
            storage.Store("SlideShowListSlidesDisplayType", value);
        }
    }
}