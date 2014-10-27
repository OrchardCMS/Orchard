using System;
using System.Globalization;

namespace Orchard.Layouts.Services {
    public class CultureAccessor : ICultureAccessor {
        private readonly IWorkContextAccessor _wca;
        private readonly Lazy<CultureInfo> _currentCulture;

        public CultureAccessor(IWorkContextAccessor wca) {
            _wca = wca;
            _currentCulture = new Lazy<CultureInfo>(() => new CultureInfo(_wca.GetContext().CurrentCulture));
        }

        public CultureInfo CurrentCulture {
            get { return _currentCulture.Value; }
        }
    }
}