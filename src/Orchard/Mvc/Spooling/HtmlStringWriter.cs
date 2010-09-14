using System.IO;
using System.Text;
using System.Web;

namespace Orchard.Mvc.Spooling {
    public class HtmlStringWriter : TextWriter ,IHtmlString {
        private readonly TextWriter _writer;

        public HtmlStringWriter() {
            _writer = new StringWriter();
        }

        public override Encoding Encoding {
            get { return _writer.Encoding; }
        }

        public string ToHtmlString() {
            return _writer.ToString();
        }

        public override string ToString() {
            return _writer.ToString();
        }

        public override void Write(string value) {
            _writer.Write(value);
        }

        public override void Write(char value) {
            _writer.Write(value);
        }
    }
}
