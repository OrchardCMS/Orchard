using System.Collections.Generic;
using Orchard.Localization;

namespace Orchard.Environment.Extensions {
    public interface ICriticalErrorProvider {
        IEnumerable<LocalizedString> GetErrors();
    }
}
