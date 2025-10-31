using Orchard.Localization;

namespace Orchard.DynamicForms.Activities
{
    public class DynamicFormSubmittedActivity : DynamicFormActivity
    {

        public const string EventName = "DynamicFormSubmitted";

        public override string Name => EventName;

        public override LocalizedString Description => T("A dynamic form is submitted.");
    }
}