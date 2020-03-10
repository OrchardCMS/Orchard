using System.Collections.Generic;


namespace Orchard.Localization.Services {
    public interface ILocalizedStringManager : IDependency {
        //string GetLocalizedString(string scope, string text, string cultureName);
        System.Tuple<string, string> GetLocalizedString(IEnumerable<string> scopes, string text, string cultureName);
    }
}
