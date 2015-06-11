using System;
using Orchard.ContentManagement;
using Orchard.Localization;

namespace IDeliverable.Slides.Services
{
    public class Updater : IUpdateModel
    {
        private readonly IUpdateModel _thunk;
        private readonly Func<string, string> _prefix;

        public Updater(IUpdateModel thunk, Func<string, string> prefix)
        {
            _thunk = thunk;
            _prefix = prefix;
        }

        public Updater(IUpdateModel thunk, string prefix)
            : this(thunk, x => String.Format("{0}.{1}", prefix, x))
        {
        }

        public bool TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) where TModel : class
        {
            return _thunk.TryUpdateModel(model, _prefix(prefix), includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage)
        {
            _thunk.AddModelError(_prefix(key), errorMessage);
        }
    }
}