using System.Globalization;

namespace Orchard.Layouts.Services {
    public class CultureAccessor : ICultureAccessor {
        private readonly IWorkContextAccessor _wca;
        public CultureAccessor(IWorkContextAccessor wca) {
            _wca = wca;
        }

        public CultureInfo CurrentCulture {
            get { return new CultureInfo(_wca.GetContext().CurrentCulture); }
        }
    }
}