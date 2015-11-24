using System.Web;

namespace Orchard.DisplayManagement {
    public class PositionWrapper : IHtmlString, IPositioned {

        private IHtmlString _value;
        public string Position { get; private set; }

        public PositionWrapper(IHtmlString value, string position) {
            _value = value;
            Position = position;
        }

        public PositionWrapper(string value, string position)
            : this(new HtmlString(HttpUtility.HtmlEncode(value)), position) {
        }

        public string ToHtmlString() {
            return _value.ToHtmlString();
        }

        public override string ToString() {
            return _value.ToString();
        }
    }
}
