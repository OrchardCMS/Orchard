using IDeliverable.Slides.Services;

namespace IDeliverable.Slides.Helpers
{
    public static class StorageExtensions
    {
        public static string RetrieveSlidesData(this IStorage storage)
        {
            return storage.Retrieve<string>("SlideshowSlides");
        }

        public static void StoreSlidesData(this IStorage storage, string value)
        {
            storage.Store("SlideshowSlides", value);
        }

        public static int? RetrieveQueryId(this IStorage storage)
        {
            return storage.Retrieve<int?>("SlideshowQueryId");
        }

        public static void StoreQueryId(this IStorage storage, int? value)
        {
            storage.Store("SlideshowQueryId", value);
        }

        public static string RetrieveProjectionSlidesDisplayType(this IStorage storage) {
            return storage.Retrieve<string>("SlideshowProjectionSlidesDisplayType");
        }

        public static void StoreProjectionSlidesDisplayType(this IStorage storage, string value)
        {
            storage.Store("SlideshowProjectionSlidesDisplayType", value);
        }

        public static int? RetrieveListId(this IStorage storage)
        {
            return storage.Retrieve<int?>("SlideshowListId");
        }

        public static void StoreListId(this IStorage storage, int? value)
        {
            storage.Store("SlideshowListId", value);
        }

        public static string RetrieveListSlidesDisplayType(this IStorage storage)
        {
            return storage.Retrieve<string>("SlideshowListSlidesDisplayType");
        }

        public static void StoreListSlidesDisplayType(this IStorage storage, string value)
        {
            storage.Store("SlideshowListSlidesDisplayType", value);
        }
    }
}