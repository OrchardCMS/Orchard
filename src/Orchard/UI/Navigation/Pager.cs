namespace Orchard.UI.Navigation {
    public class Pager {
        private const int PageDefault = 1;
        public const int PageSizeDefault = 10;
        private int _pageSize;
        private int _size;

        public int Page {
            get { return _pageSize > 0 ? _pageSize : PageDefault; }
            set { _pageSize = value; }
        }

        public int PageSize {
            get { return _size > 0 ? _size : PageSizeDefault; }
            set { _size = value; }
        }

        public int GetStartIndex(int? page = null) {
            return ((page ?? Page) - 1)*PageSize;
        }
    }
}