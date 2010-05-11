using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Web;
using System.Web.Hosting;
using System.Xml.Linq;
using Castle.Core.Logging;
using HtmlAgilityPack;
using log4net.Appender;
using log4net.Core;
using log4net.Repository;
using Orchard.Specs.Hosting;
using Orchard.Specs.Util;
using TechTalk.SpecFlow;
using NUnit.Framework;

namespace Orchard.Specs.Bindings {
    [Binding]
    public class WebAppHosting {
        private WebHost _webHost;
        private RequestDetails _details;
        private HtmlDocument _doc;
        private MessageSink _messages;

        public WebHost Host {
            get { return _webHost; }
        }

        public RequestDetails Details {
            get { return _details; }
            set { _details = value; }
        }

        [Given(@"I have a clean site")]
        public void GivenIHaveACleanSite() {
            GivenIHaveACleanSiteBasedOn("Orchard.Web");
        }


        [Given(@"I have a clean site based on (.*)")]
        public void GivenIHaveACleanSiteBasedOn(string siteFolder) {
            _webHost = new WebHost();
            Host.Initialize(siteFolder, "/");
            var shuttle = new Shuttle();
            Host.Execute(() => {
                log4net.Config.BasicConfigurator.Configure(new CastleAppender());
                HostingTraceListener.SetHook(msg => shuttle._sink.Receive(msg));
            });
            _messages = shuttle._sink;
        }

        private class CastleAppender : IAppender {
            public void Close() { }
            public string Name { get; set; }

            public void DoAppend(LoggingEvent loggingEvent) {
                var traceLoggerFactory = new TraceLoggerFactory();
                var logger = traceLoggerFactory.Create(loggingEvent.LoggerName);
                if (loggingEvent.Level <= Level.Debug)
                    logger.Debug(loggingEvent.RenderedMessage);
                else if (loggingEvent.Level <= Level.Info)
                    logger.Info(loggingEvent.RenderedMessage);
                else if (loggingEvent.Level <= Level.Warn)
                    logger.Warn(loggingEvent.RenderedMessage);
                else if (loggingEvent.Level <= Level.Error)
                    logger.Error(loggingEvent.RenderedMessage);
                else
                    logger.Fatal(loggingEvent.RenderedMessage);
            }

        }

        [Serializable]
        class Shuttle {
            public readonly MessageSink _sink = new MessageSink();
        }


        [Given(@"I have module ""(.*)""")]
        public void GivenIHaveModule(string moduleName) {
            Host.CopyExtension("Modules", moduleName);
        }

        [Given(@"I have theme ""(.*)""")]
        public void GivenIHaveTheme(string themeName) {
            Host.CopyExtension("Themes", themeName);
        }

        [Given(@"I have core ""(.*)""")]
        public void GivenIHaveCore(string moduleName) {
            Host.CopyExtension("Core", moduleName);
        }

        [Given(@"I have a clean site with")]
        public void GivenIHaveACleanSiteWith(Table table) {
            GivenIHaveACleanSite();
            foreach (var row in table.Rows) {
                foreach (var name in row["names"].Split(',').Select(x => x.Trim())) {
                    switch (row["extension"]) {
                        case "core":
                            GivenIHaveCore(name);
                            break;
                        case "module":
                            GivenIHaveModule(name);
                            break;
                        case "theme":
                            GivenIHaveTheme(name);
                            break;
                        default:
                            Assert.Fail("Unknown extension type {0}", row["extension"]);
                            break;
                    }
                }
            }
        }

        [Given(@"I am on ""(.*)""")]
        public void GivenIAmOn(string urlPath) {
            WhenIGoTo(urlPath);
        }


        [When(@"I go to ""(.*)"" on host (.*)")]
        public void WhenIGoToPathOnHost(string urlPath, string host) {
            Host.HostName = host;
            Details = Host.SendRequest(urlPath);
            _doc = new HtmlDocument();
            _doc.Load(new StringReader(Details.ResponseText));
        }

        [When(@"I go to ""(.*)""")]
        public void WhenIGoTo(string urlPath) {
            Details = Host.SendRequest(urlPath);
            _doc = new HtmlDocument();
            _doc.Load(new StringReader(Details.ResponseText));
        }

        [When(@"I follow ""(.*)""")]
        public void WhenIFollow(string linkText) {
            var link = _doc.DocumentNode
                .SelectNodes("//a")
                .Single(elt => elt.InnerText == linkText);

            var urlPath = link.Attributes["href"].Value;

            WhenIGoTo(urlPath);
        }

        [When(@"I fill in")]
        public void WhenIFillIn(Table table) {
            var inputs = _doc.DocumentNode
                .SelectNodes("//input") ?? Enumerable.Empty<HtmlNode>();

            foreach (var row in table.Rows) {
                var r = row;
                var input = inputs.First(x => x.GetAttributeValue("name", x.GetAttributeValue("id", "")) == r["name"]);
                Assert.That(input, Is.Not.Null, "Unable to locate <input> name {0} in page html:\r\n\r\n{1}", r["name"], Details.ResponseText);
                var inputType = input.Attributes["type"].Value.ToLowerInvariant();
                switch(inputType) {
                    case "radio":
                        var radios = inputs.Where(
                            x =>
                            x.GetAttributeValue("type", "") == "radio" &&
                            x.GetAttributeValue("name", x.GetAttributeValue("id", "")) == r["name"]);
                        foreach(var radio in radios) {
                            if (radio.GetAttributeValue("value", "") == row["value"])
                                radio.Attributes.Add("checked", "checked");
                            else if (radio.Attributes.Contains("checked"))
                                radio.Attributes.Remove("checked");
                        }
                        break;
                    default:
                        input.Attributes.Add("value", row["value"]);
                        break;
                }
            }
        }

        [When(@"I hit ""(.*)""")]
        public void WhenIHit(string submitText) {
            var submit = _doc.DocumentNode
                .SelectNodes("//input[@type='submit']")
                .Single(elt => elt.GetAttributeValue("value", null) == submitText);

            var form = Form.LocateAround(submit);
            var urlPath = form.Start.GetAttributeValue("action", Details.UrlPath);
            var inputs = form.Children
                    .SelectMany(elt => elt.DescendantsAndSelf("input"))
                    .GroupBy(elt => elt.GetAttributeValue("name", elt.GetAttributeValue("id", "")), elt => elt.GetAttributeValue("value", ""))
                    .ToDictionary(elt => elt.Key, elt => (IEnumerable<string>)elt);

            Details = Host.SendRequest(urlPath, inputs);
            _doc = new HtmlDocument();
            _doc.Load(new StringReader(Details.ResponseText));
        }

        [When(@"I am redirected")]
        public void WhenIAmRedirected() {
            var urlPath = "";
            if (Details.ResponseHeaders.TryGetValue("Location", out urlPath)) {
                WhenIGoTo(urlPath);
            }
            else {
                Assert.Fail("No Location header returned");
            }
        }

        [Then(@"the status should be (.*) (.*)")]
        public void ThenTheStatusShouldBe(int statusCode, string statusDescription) {
            Assert.That(Details.StatusCode, Is.EqualTo(statusCode));
            Assert.That(Details.StatusDescription, Is.EqualTo(statusDescription));
        }

        [Then(@"I should see ""(.*)""")]
        public void ThenIShouldSee(string text) {
            Assert.That(Details.ResponseText, Is.StringContaining(text));
        }

        [Then(@"I should not see ""(.*)""")]
        public void ThenIShouldNotSee(string text) {
            Assert.That(Details.ResponseText, Is.Not.StringContaining(text));
        }

        [Then(@"the title contains ""(.*)""")]
        public void ThenTheTitleContainsText(string text) {
            ScenarioContext.Current.Pending();
        }
    }

    public class Form {
        public static Form LocateAround(HtmlNode cornerstone) {
            foreach (var inspect in cornerstone.AncestorsAndSelf()) {

                var form = inspect.PreviousSiblingsAndSelf().FirstOrDefault(
                    n => n.NodeType == HtmlNodeType.Element && n.Name == "form");
                if (form == null)
                    continue;

                var endForm = inspect.NextSiblingsAndSelf().FirstOrDefault(
                    n => n.NodeType == HtmlNodeType.Text && n.InnerText == "</form>");
                if (endForm == null)
                    continue;

                return new Form {
                    Start = form,
                    End = endForm,
                    Children = form.NextSibling.NextSiblingsAndSelf().TakeWhile(n => n != endForm).ToArray()
                };
            }

            return null;
        }


        public HtmlNode Start { get; set; }
        public HtmlNode End { get; set; }
        public IEnumerable<HtmlNode> Children { get; set; }
    }

    static class HtmlExtensions {
        public static IEnumerable<HtmlNode> PreviousSiblingsAndSelf(this HtmlNode node) {
            var scan = node;
            while (scan != null) {
                yield return scan;
                scan = scan.PreviousSibling;
            }
        }
        public static IEnumerable<HtmlNode> NextSiblingsAndSelf(this HtmlNode node) {
            var scan = node;
            while (scan != null) {
                yield return scan;
                scan = scan.NextSibling;
            }
        }
    }
}
