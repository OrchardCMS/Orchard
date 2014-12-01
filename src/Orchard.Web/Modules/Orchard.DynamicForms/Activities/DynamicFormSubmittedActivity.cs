using Orchard.Localization;

namespace Orchard.DynamicForms.Activities {
    public class DynamicFormSubmittedActivity : DynamicFormActivity {

        public const string EventName = "DynamicFormSubmitted";

        public override string Name {
            get { return EventName; }
        }

        public override LocalizedString Description {
            get { return T("A dynamic form is submitted."); }
        }
    }
}