using System.Web.Mvc;
using NUnit.Framework;
using Orchard.DynamicForms.Services.Models;
using Orchard.DynamicForms.ValidationRules;

namespace Orchard.Tests.Modules.DynamicForms.ValidationRules {
    [TestFixture]
    public class EmailValidationTests {
        [SetUp]
        public void Init() {
            _context = new ValidateInputContext {ModelState = new ModelStateDictionary(), FieldName = "Email Address"};
            _validator = new EmailAddress();
        }

        private ValidateInputContext _context;
        private EmailAddress _validator;

        [Test]
        public void InvalidateDoubleDotDomain() {
            _context.AttemptedValue = "x@example..com";

            _validator.Validate(_context);

            Assert.That(_context.ModelState.IsValid, Is.False);
        }

        [Test]
        public void InvalidateMissingAt() {
            _context.AttemptedValue = "x.example.com";

            _validator.Validate(_context);

            Assert.That(_context.ModelState.IsValid, Is.False);
        }

        [Test]
        public void InvalidateMissingDomain() {
            _context.AttemptedValue = "x@";

            _validator.Validate(_context);

            Assert.That(_context.ModelState.IsValid, Is.False);
        }

        [Test]
        public void InvalidateMissingLocalPart() {
            _context.AttemptedValue = "@example.com";

            _validator.Validate(_context);

            Assert.That(_context.ModelState.IsValid, Is.False);
        }

        [Test]
        public void ValidateMissingTLD() {
            _context.AttemptedValue = "something@localhost";

            _validator.Validate(_context);

            Assert.That(_context.ModelState.IsValid, Is.True);
        }

        [Test]
        public void ValidateOneLetterTLD() {
            _context.AttemptedValue = "something@example.x";

            _validator.Validate(_context);

            Assert.That(_context.ModelState.IsValid, Is.True);
        }

        [Test]
        public void ValidateTenLetterTLD() {
            _context.AttemptedValue = "something@example.accountant";

            _validator.Validate(_context);

            Assert.That(_context.ModelState.IsValid, Is.True);
        }

        [Test]
        public void ValidateThreeLetterTLD() {
            _context.AttemptedValue = "something@example.com";

            _validator.Validate(_context);

            Assert.That(_context.ModelState.IsValid, Is.True);
        }

        [Test]
        public void ValidateTwoLetterTLD() {
            _context.AttemptedValue = "something@example.io";

            _validator.Validate(_context);

            Assert.That(_context.ModelState.IsValid, Is.True);
        }
    }
}