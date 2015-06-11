using System.Text;

namespace Orchard.UI.PageClass {
    public class PageClassBuilder : IPageClassBuilder {
        private readonly StringBuilder _classNameBuilder;

        public PageClassBuilder() {
            _classNameBuilder = new StringBuilder();
        }

        public void AddClassNames(params object[] classNames) {
            if (classNames == null)
                return;

            foreach (var className in classNames) {
                if (className == null)
                    continue;

                if (_classNameBuilder.Length > 0)
                    _classNameBuilder.AppendFormat(" {0}", className);
                else
                    _classNameBuilder.Append(className);
            }
        }

        public override string ToString() {
            return _classNameBuilder.ToString().ToLower().Replace('.', '-'); // <- just keeping it simple for now, assuming what was passed is is pretty good with '.'s to be the only invalid class name chars in module/area names
        }
    }
}