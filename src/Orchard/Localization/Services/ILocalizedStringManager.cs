using System.Collections.Generic;


namespace Orchard.Localization.Services {
    public interface ILocalizedStringManager : IDependency {
        FormatForScope GetLocalizedString(IEnumerable<string> scopes, string text, string cultureName);
    }
}
