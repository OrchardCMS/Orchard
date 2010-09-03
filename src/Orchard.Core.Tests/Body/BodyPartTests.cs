using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using JetBrains.Annotations;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Security;
using Orchard.Tests.Modules;
using Orchard.UI.Notify;

namespace Orchard.Core.Tests.Body {
    [TestFixture]
    public class BodyPartTests : DatabaseEnabledTestsBase {

        public override void Register(ContainerBuilder builder) {
            builder.RegisterType<DefaultContentManager>().As<IContentManager>();
            builder.RegisterType<DefaultContentManagerSession>().As<IContentManagerSession>();
            builder.RegisterInstance(new Mock<IContentDefinitionManager>().Object);
            builder.RegisterInstance(new Mock<ITransactionManager>().Object);
            builder.RegisterInstance(new Mock<IAuthorizer>().Object);
            builder.RegisterInstance(new Mock<INotifier>().Object);
            builder.RegisterType<OrchardServices>().As<IOrchardServices>();

            builder.RegisterType<ThingHandler>().As<IContentHandler>();

            builder.RegisterType<DefaultContentQuery>().As<IContentQuery>();
            builder.RegisterType<BodyPartHandler>().As<IContentHandler>();
        }

        [Test]
        public void BodyCanHandleLongText() {
            var contentManager = _container.Resolve<IContentManager>();

            contentManager.Create<Thing>(ThingDriver.ContentType.Name, t => {
                t.As<BodyPart>().Record = new BodyPartRecord();
                t.Text = new String('x', 10000);
            });

            var bodies = contentManager.Query<BodyPart>().List();
            Assert.That(bodies, Is.Not.Null);
            Assert.That(bodies.Any(), Is.True);
            Assert.That(bodies.First().Text, Is.EqualTo(new String('x', 10000)));

        }

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] {
                                 typeof(BodyPartRecord), 
                                 typeof(ContentTypeRecord),
                                 typeof(ContentItemRecord), 
                                 typeof(ContentItemVersionRecord), 
                                 typeof(CommonPartRecord),
                                 typeof(CommonPartVersionRecord),
                             };
            }
        }

        [UsedImplicitly]
        public class ThingHandler : ContentHandler {
            public ThingHandler() {
                Filters.Add(new ActivatingFilter<Thing>(ThingDriver.ContentType.Name));
                Filters.Add(new ActivatingFilter<ContentPart<CommonPartVersionRecord>>(ThingDriver.ContentType.Name));
                Filters.Add(new ActivatingFilter<CommonPart>(ThingDriver.ContentType.Name));
                Filters.Add(new ActivatingFilter<BodyPart>(ThingDriver.ContentType.Name));
            }
        }

        public class Thing : ContentPart {
            public int Id { get { return ContentItem.Id; } }

            public string Text {
                get { return this.As<BodyPart>().Text; }
                set { this.As<BodyPart>().Text = value; }
            }

        }

        public class ThingDriver : ContentPartDriver<Thing> {
            public readonly static ContentType ContentType = new ContentType {
                Name = "thing",
                DisplayName = "Thing"
            };
        }

    }
}