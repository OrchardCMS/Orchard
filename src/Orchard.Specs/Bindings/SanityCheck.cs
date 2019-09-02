using NUnit.Framework;
using TechTalk.SpecFlow;

namespace Orchard.Specs.Bindings {
    [Binding]
    public class SanityCheck {
        private bool _runSteps;
        private bool _valueSet;
        private int _value;

        [Given("I have a scenario")]
        public void GivenIHaveAScenario() {

        }

        [When("I run steps")]
        public void WhenIRunSteps() {
            _runSteps = true;
        }


        [When(@"they have values like ""(.*)""")]
        public void WhenTheyHaveValuesLike(int value) {
            Assert.That(_valueSet, Is.False);
            _value = value;
            _valueSet = true;
        }

        [Then("they run")]
        public void ThenTheyRun() {
            Assert.That(_runSteps, Is.True);
        }

        [Then("values like five are captured")]
        public void ThenValuesLikeFiveAreCaptured() {
            Assert.That(_valueSet, Is.True);
            Assert.That(_value, Is.EqualTo(5));
        }
    }
}
