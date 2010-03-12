namespace Orchard.Mvc.Html {
    public static class FileRegistrationContextExtensions {
        public static T WithCondition<T>(this T fileRegistrationContext, string condition)where T : FileRegistrationContext {
            if (fileRegistrationContext == null)
                return null;

            fileRegistrationContext.Condition = condition;
            return fileRegistrationContext;
        }

        public static T ForMedia<T>(this T fileRegistrationContext, string media) where T : FileRegistrationContext {
            if (fileRegistrationContext == null)
                return null;

            fileRegistrationContext.SetAttribute("media", media);
            return fileRegistrationContext;
        }
    }
}