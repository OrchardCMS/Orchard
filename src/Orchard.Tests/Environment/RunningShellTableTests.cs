using NUnit.Framework;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Environment {
    [TestFixture]
    public class RunningShellTableTests {
        [Test]
        public void NoShellsGiveNoMatch() {
            var table = new RunningShellTable();
            var match = table.Match(new StubHttpContext());
            Assert.That(match, Is.Null);
        }

        [Test]
        public void DefaultShellMatchesByDefault() {
            var table = (IRunningShellTable)new RunningShellTable();
            var settings = new ShellSettings { Name = ShellSettings.DefaultName };
            table.Add(settings);
            var match = table.Match(new StubHttpContext());
            Assert.That(match, Is.SameAs(settings));
        }

        [Test]
        public void AnotherShellMatchesByHostHeader() {
            var table = (IRunningShellTable)new RunningShellTable();
            var settings = new ShellSettings { Name = ShellSettings.DefaultName };
            var settingsA = new ShellSettings { Name = "Alpha", RequestUrlHost = "a.example.com" };
            table.Add(settings);
            table.Add(settingsA);
            var match = table.Match(new StubHttpContext("~/foo/bar", "a.example.com"));
            Assert.That(match, Is.SameAs(settingsA));
        }

        [Test]
        public void DefaultStillCatchesWhenOtherShellsMiss() {
            var table = (IRunningShellTable)new RunningShellTable();
            var settings = new ShellSettings { Name = ShellSettings.DefaultName };
            var settingsA = new ShellSettings { Name = "Alpha", RequestUrlHost = "a.example.com" };
            table.Add(settings);
            table.Add(settingsA);
            var match = table.Match(new StubHttpContext("~/foo/bar", "b.example.com"));
            Assert.That(match, Is.SameAs(settings));
        }

        [Test]
        public void DefaultWontFallbackIfItHasCriteria() {
            var table = (IRunningShellTable)new RunningShellTable();
            var settings = new ShellSettings { Name = ShellSettings.DefaultName, RequestUrlHost = "www.example.com" };
            var settingsA = new ShellSettings { Name = "Alpha", RequestUrlHost = "a.example.com" };
            table.Add(settings);
            table.Add(settingsA);
            var match = table.Match(new StubHttpContext("~/foo/bar", "b.example.com"));
            Assert.That(match, Is.Null);
        }

        [Test]
        public void DefaultWillCatchRequestsIfItMatchesCriteria() {
            var table = (IRunningShellTable)new RunningShellTable();
            var settings = new ShellSettings { Name = ShellSettings.DefaultName, RequestUrlHost = "www.example.com" };
            var settingsA = new ShellSettings { Name = "Alpha", RequestUrlHost = "a.example.com" };
            table.Add(settings);
            table.Add(settingsA);
            var match = table.Match(new StubHttpContext("~/foo/bar", "www.example.com"));
            Assert.That(match, Is.EqualTo(settings));
        }

        [Test]
        public void NonDefaultCatchallWillFallbackIfNothingElseMatches() {
            var table = (IRunningShellTable)new RunningShellTable();
            var settings = new ShellSettings { Name = ShellSettings.DefaultName, RequestUrlHost = "www.example.com" };
            var settingsA = new ShellSettings { Name = "Alpha" };
            table.Add(settings);
            table.Add(settingsA);
            var match = table.Match(new StubHttpContext("~/foo/bar", "b.example.com"));
            Assert.That(match, Is.EqualTo(settingsA));
        }

        [Test]
        public void DefaultCatchallIsFallbackEvenWhenOthersAreUnqualified() {
            var table = (IRunningShellTable)new RunningShellTable();
            var settings = new ShellSettings { Name = ShellSettings.DefaultName };
            var settingsA = new ShellSettings { Name = "Alpha" };
            var settingsB = new ShellSettings { Name = "Beta", RequestUrlHost = "b.example.com" };
            var settingsG = new ShellSettings { Name = "Gamma" };
            table.Add(settings);
            table.Add(settingsA);
            table.Add(settingsB);
            table.Add(settingsG);
            var match = table.Match(new StubHttpContext("~/foo/bar", "a.example.com"));
            Assert.That(match, Is.EqualTo(settings));
        }

        [Test]
        public void ThereIsNoFallbackIfMultipleSitesAreUnqualifiedButDefaultIsNotOneOfThem() {
            var table = (IRunningShellTable)new RunningShellTable();
            var settings = new ShellSettings { Name = ShellSettings.DefaultName, RequestUrlHost = "www.example.com" };
            var settingsA = new ShellSettings { Name = "Alpha" };
            var settingsB = new ShellSettings { Name = "Beta", RequestUrlHost = "b.example.com" };
            var settingsG = new ShellSettings { Name = "Gamma" };
            table.Add(settings);
            table.Add(settingsA);
            table.Add(settingsB);
            table.Add(settingsG);
            var match = table.Match(new StubHttpContext("~/foo/bar", "a.example.com"));
            Assert.That(match, Is.Null);
        }

        [Test]
        public void PathAlsoCausesMatch() {
            var table = (IRunningShellTable)new RunningShellTable();
            var settings = new ShellSettings { Name = ShellSettings.DefaultName };
            var settingsA = new ShellSettings { Name = "Alpha", RequestUrlPrefix = "~/foo" };
            table.Add(settings);
            table.Add(settingsA);
            var match = table.Match(new StubHttpContext("~/foo/bar", "a.example.com"));
            Assert.That(match, Is.SameAs(settingsA));
        }

        [Test]
        public void PathAndHostMustBothMatch() {
            var table = (IRunningShellTable)new RunningShellTable();
            var settings = new ShellSettings { Name = ShellSettings.DefaultName, RequestUrlHost = "www.example.com", };
            var settingsA = new ShellSettings { Name = "Alpha", RequestUrlHost = "wiki.example.com", RequestUrlPrefix = "~/foo" };
            var settingsB = new ShellSettings { Name = "Beta", RequestUrlHost = "wiki.example.com", RequestUrlPrefix = "~/bar" };
            var settingsG = new ShellSettings { Name = "Gamma", RequestUrlHost = "wiki.example.com" };
            var settingsD = new ShellSettings { Name = "Delta", RequestUrlPrefix = "~/Quux" };
            table.Add(settings);
            table.Add(settingsA);
            table.Add(settingsB);
            table.Add(settingsG);
            table.Add(settingsD);

            Assert.That(table.Match(new StubHttpContext("~/foo/bar", "wiki.example.com")), Is.SameAs(settingsA));
            Assert.That(table.Match(new StubHttpContext("~/bar/foo", "wiki.example.com")), Is.SameAs(settingsB));
            Assert.That(table.Match(new StubHttpContext("~/baaz", "wiki.example.com")), Is.SameAs(settingsG));
            Assert.That(table.Match(new StubHttpContext("~/foo/bar", "www.example.com")), Is.SameAs(settings));
            Assert.That(table.Match(new StubHttpContext("~/bar/foo", "www.example.com")), Is.SameAs(settings));
            Assert.That(table.Match(new StubHttpContext("~/baaz", "www.example.com")), Is.SameAs(settings));
            Assert.That(table.Match(new StubHttpContext("~/foo/bar", "a.example.com")), Is.Null);

            Assert.That(table.Match(new StubHttpContext("~/quux/quad", "wiki.example.com")), Is.SameAs(settingsG));
            Assert.That(table.Match(new StubHttpContext("~/quux/quad", "www.example.com")), Is.SameAs(settings));
            Assert.That(table.Match(new StubHttpContext("~/quux/quad", "a.example.com")), Is.SameAs(settingsD));
            Assert.That(table.Match(new StubHttpContext("~/yarg", "wiki.example.com")), Is.SameAs(settingsG));
            Assert.That(table.Match(new StubHttpContext("~/yarg", "www.example.com")), Is.SameAs(settings));
            Assert.That(table.Match(new StubHttpContext("~/yarg", "a.example.com")), Is.Null);
        }

        [Test]
        public void PathAloneWillMatch() {
            var table = (IRunningShellTable)new RunningShellTable();
            var settingsA = new ShellSettings { Name = "Alpha", RequestUrlPrefix = "~/foo" };
            table.Add(settingsA);

            Assert.That(table.Match(new StubHttpContext("~/foo/bar", "wiki.example.com")), Is.SameAs(settingsA));
            Assert.That(table.Match(new StubHttpContext("~/bar/foo", "wiki.example.com")), Is.Null);
        }

        [Test]
        public void HostNameMatchesRightmostIfRequestIsLonger() {
            var table = (IRunningShellTable) new RunningShellTable();
            var settings = new ShellSettings { Name = ShellSettings.DefaultName };
            var settingsA = new ShellSettings { Name = "Alpha", RequestUrlHost = "example.com" };
            table.Add(settings);
            table.Add(settingsA);
            Assert.That(table.Match(new StubHttpContext("~/foo/bar", "www.example.com")), Is.SameAs(settingsA));
            Assert.That(table.Match(new StubHttpContext("~/foo/bar", "wiki.example.com")), Is.SameAs(settingsA));
            Assert.That(table.Match(new StubHttpContext("~/foo/bar", "example.com")), Is.SameAs(settingsA));
            Assert.That(table.Match(new StubHttpContext("~/foo/bar", "localhost")), Is.SameAs(settings));
        }

        [Test]
        public void LongestMatchingHostHasPriority() {
            var table = (IRunningShellTable) new RunningShellTable();
            var settings = new ShellSettings { Name = ShellSettings.DefaultName };
            var settingsA = new ShellSettings { Name = "Alpha", RequestUrlHost = "www.example.com" };
            var settingsB = new ShellSettings { Name = "Beta", RequestUrlHost = "example.com" };
            var settingsG = new ShellSettings { Name = "Gamma", RequestUrlHost = "wiki.example.com" };
            table.Add(settings);
            table.Add(settingsA);
            table.Add(settingsB);
            table.Add(settingsG);

            Assert.That(table.Match(new StubHttpContext("~/foo/bar", "www.example.com")), Is.SameAs(settingsA));
            Assert.That(table.Match(new StubHttpContext("~/foo/bar", "wiki.example.com")), Is.SameAs(settingsG));
            Assert.That(table.Match(new StubHttpContext("~/foo/bar", "username.example.com")), Is.SameAs(settingsB));
            Assert.That(table.Match(new StubHttpContext("~/foo/bar", "localhost")), Is.SameAs(settings));
        }


        [Test]
        public void ShellNameUsedToDistinctThingsAsTheyAreAdded() {
            var table = (IRunningShellTable)new RunningShellTable();
            var settings = new ShellSettings { Name = ShellSettings.DefaultName };
            var settingsA = new ShellSettings { Name = "Alpha", RequestUrlHost = "removed.example.com" };
            var settingsB = new ShellSettings { Name = "Alpha", RequestUrlHost = "added.example.com" };
            table.Add(settings);
            table.Add(settingsA);
            table.Add(settingsB);

            Assert.That(table.Match(new StubHttpContext("~/foo/bar", "removed.example.com")), Is.SameAs(settings));
            Assert.That(table.Match(new StubHttpContext("~/foo/bar", "added.example.com")), Is.SameAs(settingsB));
            Assert.That(table.Match(new StubHttpContext("~/foo/bar", "localhost")), Is.SameAs(settings));
        }
    }
}