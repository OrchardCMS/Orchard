using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Orchard.CmsPages.Models;
using Orchard.CmsPages.Services;
using Orchard.CmsPages.Services.Templates;
using Orchard.Data;

namespace Orchard.Tests.Packages.Pages.Services {
    [TestFixture]
    public class PageSchedulerTests : DatabaseEnabledTestsBase {
        private IPageScheduler _scheduler;
        private IPageManager _manager;


        public override void Init() {
            base.Init();
            _manager = _container.Resolve<IPageManager>();
            _scheduler = _container.Resolve<IPageScheduler>();
        }

        public override void Register(Autofac.Builder.ContainerBuilder builder) {
            builder.Register<PageScheduler>().As<IPageScheduler>();
            builder.Register<PageManager>().As<IPageManager>();
            builder.Register(new StubTemplateProvider()).As<ITemplateProvider>();
        }

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] {
                                 typeof (Page), typeof (PageRevision), typeof (ContentItem), typeof (Published),
                                 typeof (Scheduled)
                             };
            }
        }

        private class StubTemplateProvider : ITemplateProvider {
            public IList<TemplateDescriptor> List() {
                return Enumerable.Empty<TemplateDescriptor>().ToList();
            }

            public TemplateDescriptor Get(string name) {
                return null;
            }
        }

        [Test]
        public void SchedulerWillStoreScheduledTasksThatCanBeListedFromTheRevisionAndPage() {
            var published = _manager.CreateAndPublishPage("hello", "Hello World");
            var draft = _manager.AcquireDraft(published.Page.Id);
            var futureMoment1 = _clock.FutureMoment(TimeSpan.FromMinutes(5));
            var futureMoment2 = _clock.FutureMoment(TimeSpan.FromMinutes(6));
            _scheduler.AddPublishTask(draft, futureMoment1);
            _scheduler.AddPublishTask(published, futureMoment2);
            _session.Flush();
            _session.Clear();

            var draft2 = _manager.GetLastRevision(published.Page.Id);
            var published2 = _manager.GetPublishedBySlug("hello");

            //each has one scheduled task
            Assert.That(draft2.Scheduled, Has.Count.EqualTo(1));
            Assert.That(published2.Scheduled, Has.Count.EqualTo(1));

            // both have same page, which has two publish tasks
            Assert.That(draft2.Page, Is.SameAs(published2.Page));
            Assert.That(draft2.Page.Scheduled, Has.Count.EqualTo(2));
            Assert.That(published2.Page.Scheduled, Has.Count.EqualTo(2));
            Assert.That(published2.Page.Scheduled, Has.All.Property("Action").EqualTo(ScheduledAction.Publish));

            // time of each task is correct
            Assert.That(draft2.Scheduled.Single().ScheduledDate, Is.EqualTo(futureMoment1));
            Assert.That(published2.Scheduled.Single().ScheduledDate, Is.EqualTo(futureMoment2));
        }

        [Test]
        public void SweepShouldPublishAndRemoveTaskRecordsWhenTheApproprioateTimeHasPassed() {
            
            var published = _manager.CreateAndPublishPage("hello", "Hello World");
            var draft = _manager.AcquireDraft(published.Page.Id);
            var moment = _clock.FutureMoment(TimeSpan.FromMinutes(5));
            
            Assert.That(draft.Id, Is.Not.EqualTo(published.Id));
            
            _scheduler.AddPublishTask(draft, moment);

            _session.Flush();
            _scheduler.Sweep();

            Assert.That(draft.Scheduled, Has.Count.EqualTo(1));
            Assert.That(_manager.GetPublishedBySlug("hello"), Has.Property("Id").EqualTo(published.Id));

            _clock.Advance(TimeSpan.FromMinutes(6));
            _session.Flush();
            _scheduler.Sweep();

            Assert.That(draft.Scheduled, Has.Count.EqualTo(0));
            Assert.That(_manager.GetPublishedBySlug("hello"), Has.Property("Id").EqualTo(draft.Id));
        }

        [Test]
        public void SchedulerShouldRemoveTasksIfTheyFail() {
            _scheduler.AddPublishTask(
                _manager.CreateAndPublishPage("hello", "Hello World"),
                _clock.FutureMoment(TimeSpan.FromMinutes(5)));

            // undefined action should throw an exception 
            _session.Flush();
            _container.Resolve<IRepository<Scheduled>>().Table.Single().Action = ScheduledAction.Undefined;

            _scheduler.Sweep();
            
            _session.Flush();
            Assert.That(_container.Resolve<IRepository<Scheduled>>().Count(x => true), Is.EqualTo(1));

            _clock.Advance(TimeSpan.FromMinutes(6));
            _scheduler.Sweep();

            _session.Flush();
            Assert.That(_container.Resolve<IRepository<Scheduled>>().Count(x => true), Is.EqualTo(0));
        }

    }
}