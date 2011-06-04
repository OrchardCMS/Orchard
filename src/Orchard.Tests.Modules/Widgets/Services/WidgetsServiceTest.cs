using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Models;
using Orchard.Core.Settings.Metadata.Records;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Features;
using Orchard.Security;
using Orchard.Tests.Stubs;
using Orchard.UI.Notify;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;

namespace Orchard.Tests.Modules.Widgets.Services {

    [TestFixture]
    public class WidgetsServiceTest : DatabaseEnabledTestsBase {

        private const string ThemeZoneName1 = "sidebar";
        private const string ThemeZoneName2 = "alternative";
        private const string DuplicateZoneNames = "sidebar,alternative, sidebar , alternative ";

        private const string LayerName1 = "Test layer 1";
        private const string LayerDescription1 = "Test layer 1";
        private const string LayerName2 = "Test layer 2";
        private const string LayerDescription2 = "Test layer 2";
        private const string WidgetTitle1 = "Test widget 1";
        private const string WidgetTitle2 = "Test widget 2";
        private const string WidgetTitle3 = "Test widget 3";
        private const string Zone1 = "Zone1";
        private const string Zone2 = "Zone2";
        private const string Position1 = "1";
        private const string Position2 = "3";
        private const string Position3 = "4";

        private IWidgetsService _widgetService;
        private IContentManager _contentManager;

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] {
                    typeof(LayerPartRecord),
                    typeof(WidgetPartRecord),
                    typeof(CommonPartRecord),
                    typeof(BodyPartRecord),
                    typeof(ContentPartRecord),
                    typeof(ContentTypeRecord),
                    typeof(ContentItemRecord),
                    typeof(ContentItemVersionRecord),
                    typeof(ContentTypeDefinitionRecord),
                    typeof(ContentTypePartDefinitionRecord),
                    typeof(ContentPartDefinitionRecord),
                    typeof(ContentPartFieldDefinitionRecord),
                    typeof(ContentFieldDefinitionRecord)
                };
            }
        }

        public override void Init() {
            base.Init();

            _widgetService = _container.Resolve<IWidgetsService>();
            _contentManager = _container.Resolve<IContentManager>();
        }

        public override void Register(ContainerBuilder builder) {
            var mockFeatureManager = new Mock<IFeatureManager>();

            var theme1 = new FeatureDescriptor {Extension = new ExtensionDescriptor { Zones = ThemeZoneName1, ExtensionType = "Theme" }};
            var theme2 = new FeatureDescriptor { Extension = new ExtensionDescriptor { Zones = ThemeZoneName2, ExtensionType = "Theme" } };
            var theme3 = new FeatureDescriptor { Extension = new ExtensionDescriptor { Zones = DuplicateZoneNames, ExtensionType = "Theme" } };
            var module1 = new FeatureDescriptor { Extension = new ExtensionDescriptor { Zones = "DontSeeMeBecauseIAmNotATheme", ExtensionType = "Module" } };
            mockFeatureManager.Setup(x => x.GetEnabledFeatures())
                .Returns(new[] { theme1, theme2, theme3, module1 });

            builder.RegisterType<DefaultContentManager>().As<IContentManager>();
            builder.RegisterType<DefaultContentManagerSession>().As<IContentManagerSession>();
            builder.RegisterInstance(new Mock<IContentDefinitionManager>().Object);
            builder.RegisterInstance(new Mock<ITransactionManager>().Object);
            builder.RegisterInstance(new Mock<IAuthorizer>().Object);
            builder.RegisterInstance(new Mock<INotifier>().Object);
            builder.RegisterInstance(mockFeatureManager.Object);
            builder.RegisterType<OrchardServices>().As<IOrchardServices>();
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();
            builder.RegisterType<WidgetsService>().As<IWidgetsService>();
            builder.RegisterType<StubExtensionManager>().As<IExtensionManager>();


            builder.RegisterType<StubWidgetPartHandler>().As<IContentHandler>();
            builder.RegisterType<StubLayerPartHandler>().As<IContentHandler>();
            builder.RegisterType<DefaultContentQuery>().As<IContentQuery>();
            builder.RegisterType<DefaultContentDisplay>().As<IContentDisplay>();

        }

        [Test]
        public void GetLayersTest() {
            IEnumerable<LayerPart> layers = _widgetService.GetLayers();
            Assert.That(layers.Count(), Is.EqualTo(0));

            LayerPart layerPartFirst = _widgetService.CreateLayer(LayerName1, LayerDescription1, "");
            _contentManager.Flush();

            layers = _widgetService.GetLayers();
            Assert.That(layers.Count(), Is.EqualTo(1));
            Assert.That(layers.First().Id, Is.EqualTo(layerPartFirst.Id));

            _widgetService.CreateLayer(LayerName2, LayerDescription2, "");
            _contentManager.Flush();
            Assert.That(layers.Count(), Is.EqualTo(1));
        }

        [Test]
        public void GetLayerTest() {
            IEnumerable<LayerPart> layers = _widgetService.GetLayers();
            Assert.That(layers.Count(), Is.EqualTo(0), "No layers yet");

            _widgetService.CreateLayer("Test layer 1", "Test layer 1", "");
            _contentManager.Flush();

            layers = _widgetService.GetLayers();
            Assert.That(layers.Count(), Is.EqualTo(1), "One layer was created");
        }

        [Test]
        public void CreateLayerTest() {
            IEnumerable<LayerPart> layers = _widgetService.GetLayers();
            Assert.That(layers.Count(), Is.EqualTo(0), "No layers yet");

            _widgetService.CreateLayer(LayerName1, LayerDescription1, "");
            _contentManager.Flush();

            layers = _widgetService.GetLayers();
            LayerPart layer = layers.First();
            Assert.That(layer.Record.Name, Is.EqualTo(LayerName1));
            Assert.That(layer.Record.Description, Is.EqualTo(LayerDescription1));
        }

        [Test]
        public void GetWidgetTest() {
            LayerPart layerPart = _widgetService.CreateLayer(LayerName1, LayerDescription1, "");
            _contentManager.Flush();

            WidgetPart widgetResult = _widgetService.GetWidget(0);
            Assert.That(widgetResult, Is.Null);

            WidgetPart widgetPart = _widgetService.CreateWidget(layerPart.Id, "HtmlWidget", WidgetTitle1, "1", "");
            Assert.That(widgetPart, Is.Not.Null);

            widgetResult = _widgetService.GetWidget(0);
            Assert.That(widgetResult, Is.Null, "Still yields null on an invalid identifier");

            _contentManager.Flush();
            widgetResult = _widgetService.GetWidget(widgetPart.Id);
            Assert.That(widgetResult.Id, Is.EqualTo(widgetPart.Id), "Returns correct widget");
        }

        [Test]
        public void GetWidgetsTest() {
            LayerPart layerPart = _widgetService.CreateLayer(LayerName1, LayerDescription1, "");
            _contentManager.Flush();

            IEnumerable<WidgetPart> widgetResults = _widgetService.GetWidgets();
            Assert.That(widgetResults.Count(), Is.EqualTo(0));

            WidgetPart widgetPart = _widgetService.CreateWidget(layerPart.Id, "HtmlWidget", WidgetTitle1, "1", "");
            Assert.That(widgetPart, Is.Not.Null);
            _contentManager.Flush();

            widgetResults = _widgetService.GetWidgets();
            Assert.That(widgetResults.Count(), Is.EqualTo(1));
            Assert.That(widgetResults.First().Id, Is.EqualTo(widgetPart.Id));

            _widgetService.CreateWidget(layerPart.Id, "HtmlWidget", WidgetTitle2, "2", "");
            _contentManager.Flush();

            widgetResults = _widgetService.GetWidgets();
            Assert.That(widgetResults.Count(), Is.EqualTo(2));
        }

        [Test]
        public void CreateWidgetTest() {
            LayerPart layerPart = _widgetService.CreateLayer(LayerName1, LayerDescription1, "");
            _contentManager.Flush();

            WidgetPart widgetPart = _widgetService.CreateWidget(layerPart.Id, "HtmlWidget", WidgetTitle1, "1", "");
            Assert.That(widgetPart, Is.Not.Null);
            Assert.That(widgetPart.LayerPart.Id, Is.EqualTo(layerPart.Id));
        }

        [Test]
        //[Ignore("Needs fixing")]
        public void GetZonesTest() {
            IEnumerable<string> zones = _widgetService.GetZones();
            Assert.That(zones.Count(), Is.EqualTo(2), "Two zones on the mock list");
            Assert.That(zones.FirstOrDefault(zone => zone == ThemeZoneName1), Is.Not.Null);
            Assert.That(zones.FirstOrDefault(zone => zone == ThemeZoneName2), Is.Not.Null);
        }

        [Test, Ignore("Fix when possible")]
        public void MoveWidgetTest() {
            LayerPart layerPart = _widgetService.CreateLayer(LayerName1, LayerDescription1, "");
            _contentManager.Flush();

            // same zone widgets
            WidgetPart widgetPart1 = _widgetService.CreateWidget(layerPart.Id, "HtmlWidget", WidgetTitle1, Position1, Zone1);
            WidgetPart widgetPart2 = _widgetService.CreateWidget(layerPart.Id, "HtmlWidget", WidgetTitle2, Position2, Zone1);

            // different zone widget
            WidgetPart widgetPart3 = _widgetService.CreateWidget(layerPart.Id, "HtmlWidget", WidgetTitle3, Position3, Zone2);
            _contentManager.Flush();

            // test 1 - moving first widget up will have no effect
            Assert.That(_widgetService.MoveWidgetUp(widgetPart1), Is.False);

            // test 2 - moving first widget down will be successfull
            Assert.That(_widgetService.MoveWidgetDown(widgetPart1), Is.True);

            widgetPart1 = _widgetService.GetWidget(widgetPart1.Id);
            Assert.That(widgetPart1.Position, Is.EqualTo(Position2), "First widget moved to second widget position");

            widgetPart2 = _widgetService.GetWidget(widgetPart2.Id);
            Assert.That(widgetPart2.Position, Is.EqualTo(Position1), "Second widget moved to first widget position");

            // test 3 - moving last widget down will have no effect even though there is a widget in another zone with a higher position
            Assert.That(_widgetService.MoveWidgetDown(widgetPart1), Is.False);

            widgetPart1 = _widgetService.GetWidget(widgetPart1.Id);
            Assert.That(widgetPart1.Position, Is.EqualTo(Position2), "Widget remained in the same position");

            widgetPart2 = _widgetService.GetWidget(widgetPart2.Id);
            Assert.That(widgetPart2.Position, Is.EqualTo(Position1), "Widget remained in the same position");

            widgetPart3 = _widgetService.GetWidget(widgetPart3.Id);
            Assert.That(widgetPart3.Position, Is.EqualTo(Position3), "Widget remained in the same position");
        }

        [Test, Ignore("Fix when possible")]
        public void GetLayerWidgetsTest() {
            LayerPart layerPart = _widgetService.CreateLayer(LayerName1, LayerDescription1, "");
            _contentManager.Flush();

            // same zone widgets
            WidgetPart widgetPart1 = _widgetService.CreateWidget(layerPart.Id, "HtmlWidget", WidgetTitle1, Position1, Zone1);
            WidgetPart widgetPart2 = _widgetService.CreateWidget(layerPart.Id, "HtmlWidget", WidgetTitle2, Position2, Zone1);

            // different zone widget
            _widgetService.CreateWidget(layerPart.Id, "HtmlWidget", WidgetTitle3, Position3, Zone2);

            // test 1 - moving first widget up will have no effect
            IEnumerable<WidgetPart> layerWidgets = _widgetService.GetWidgets(layerPart.Id);
            Assert.That(layerWidgets.Count(), Is.EqualTo(2));
            Assert.That(layerWidgets.Contains(widgetPart1));
            Assert.That(layerWidgets.Contains(widgetPart2));
        }

        public class StubLayerPartHandler : ContentHandler {
            public StubLayerPartHandler(IRepository<LayerPartRecord> layersRepository) {
                Filters.Add(new ActivatingFilter<LayerPart>("Layer"));
                Filters.Add(new ActivatingFilter<CommonPart>("Layer"));
                Filters.Add(StorageFilter.For(layersRepository));
            }
        }

        public class StubWidgetPartHandler : ContentHandler {
            public StubWidgetPartHandler(IRepository<WidgetPartRecord> widgetsRepository) {
                Filters.Add(new ActivatingFilter<WidgetPart>("HtmlWidget"));
                Filters.Add(new ActivatingFilter<CommonPart>("HtmlWidget"));
                Filters.Add(new ActivatingFilter<BodyPart>("HtmlWidget"));
                Filters.Add(StorageFilter.For(widgetsRepository));
            }
        }
    }
}