using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using Orchard.Specs.Hosting;
using Orchard.Specs.Util;
using TechTalk.SpecFlow;
using NUnit.Framework;

namespace Orchard.Specs.Bindings {
    [Binding]
    public class WebAppHosting {
        private WebHost _webHost;
        private RequestDetails _details;

        [Given(@"I have a clean site")]
        public void GivenIHaveACleanSite() {
            _webHost = new WebHost();
            _webHost.Initialize("Orchard.Web", "/");
        }

        [Given(@"I have module ""(.*)""")]
        public void GivenIHaveModule(string moduleName) {
            _webHost.CopyExtension("Modules", moduleName);
        }

        [Given(@"I have theme ""(.*)""")]
        public void GivenIHaveTheme(string themeName) {
            _webHost.CopyExtension("Themes", themeName);
        }

        [Given(@"I have core ""(.*)""")]
        public void GivenIHaveCore(string moduleName) {
            _webHost.CopyExtension("Core", moduleName);
        }

        [When(@"I go to ""(.*)""")]
        public void WhenIGoTo(string urlPath) {
            _details = _webHost.SendRequest(urlPath);
        }

        [When(@"I follow ""(.*)""")]
        public void WhenIFollow(string linkText) {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.Load(new StringReader(_details.ResponseText));
            var link = doc.DocumentNode.SelectNodes("//a").Single(elt => elt.InnerText == linkText);

            WhenIGoTo(link.Attributes["href"].Value);
        }

        [Then(@"the status should be (.*) (.*)")]
        public void ThenTheStatusShouldBe(int statusCode, string statusDescription) {
            Assert.That(_details.StatusCode, Is.EqualTo(statusCode));
            Assert.That(_details.StatusDescription, Is.EqualTo(statusDescription));
        }

        [Then(@"I should see ""(.*)""")]
        public void ThenIShouldSee(string text) {
            Assert.That(_details.ResponseText, Is.StringContaining(text));
        }

        [Then(@"the title contains ""(.*)""")]
        public void ThenTheTitleContainsText(string text) {
            ScenarioContext.Current.Pending();
        }
    }
}
