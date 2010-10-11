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
using Orchard.Security;
using Orchard.Themes;
using Orchard.Themes.Models;
using Orchard.UI.Notify;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;

namespace Orchard.Tests.Modules.Widgets.Services {
    
    [TestFixture]
    public class WidgetsServiceTest : DatabaseEnabledTestsBase {

        private const string ThemeZoneName1 = "sidebar";
        private const string ThemeZoneName2 = "alternative";

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
            builder.RegisterType<DefaultContentManager>().As<IContentManager>();
            builder.RegisterType<DefaultContentManagerSession>().As<IContentManagerSession>();
            builder.RegisterInstance(new Mock<IContentDefinitionManager>().Object);
            builder.RegisterInstance(new Mock<ITransactionManager>().Object);
            builder.RegisterInstance(new Mock<IAuthorizer>().Object);
            builder.RegisterInstance(new Mock<INotifier>().Object);
            builder.RegisterType<OrchardServices>().As<IOrchardServices>();
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();
            builder.RegisterType<ShapeHelperFactory>().As<IShapeHelperFactory>();
            builder.RegisterType<WidgetsService>().As<IWidgetsService>();

            Theme theme1 = new Theme { Zones = ThemeZoneName1 };
            Theme theme2 = new Theme { Zones = ThemeZoneName2 };
            Mock<IThemeService> themeServiceMock = new Mock<IThemeService>();
            themeServiceMock.Setup(x => x.GetInstalledThemes()).Returns(
                (new ITheme[] { theme1, theme2 }));

            builder.RegisterInstance(themeServiceMock.Object).As<IThemeService>();
            builder.RegisterType<StubWidgetPartHandler>().As<IContentHandler>();
            builder.RegisterType<StubLayerPartHandler>().As<IContentHandler>();
            builder.RegisterType<DefaultContentQuery>().As<IContentQuery>();
        }

        [Test]
        public void GetLayersTest() {
            const string layerName1 = "Test layer 1";
            const string layerDescription1 = "Test layer 1";
            const string layerName2 = "Test layer 2";
            const string layerDescription2 = "Test layer 2";

            IEnumerable<LayerPart> layers = _widgetService.GetLayers();
            Assert.That(layers.Count(), Is.EqualTo(0));

            LayerPart layerPartFirst = _widgetService.CreateLayer(layerName1, layerDescription1, "");
            _contentManager.Flush();

            layers = _widgetService.GetLayers();
            Assert.That(layers.Count(), Is.EqualTo(1));
            Assert.That(layers.First().Id, Is.EqualTo(layerPartFirst.Id));

            _widgetService.CreateLayer(layerName2, layerDescription2, "");
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
            const string layerName = "Test layer 1";
            const string layerDescription = "Test layer 1";

            IEnumerable<LayerPart> layers = _widgetService.GetLayers();
            Assert.That(layers.Count(), Is.EqualTo(0), "No layers yet");

            _widgetService.CreateLayer(layerName, layerDescription, "");
            _contentManager.Flush();

            layers = _widgetService.GetLayers();
            LayerPart layer = layers.First();
            Assert.That(layer.Record.Name, Is.EqualTo(layerName));
            Assert.That(layer.Record.Description, Is.EqualTo(layerDescription));
        }

        [Test]
        public void GetWidgetTest() {
            const string layerName = "Test layer 1";
            const string layerDescription = "Test layer 1";
            const string widgetTitle = "Test widget 1";

            LayerPart layerPart = _widgetService.CreateLayer(layerName, layerDescription, "");
            _contentManager.Flush();

            WidgetPart widgetResult = _widgetService.GetWidget(0);
            Assert.That(widgetResult, Is.Null);

            WidgetPart widgetPart = _widgetService.CreateWidget(layerPart.Id, "HtmlWidget", widgetTitle, "1", "");
            Assert.That(widgetPart, Is.Not.Null);
            
            widgetResult = _widgetService.GetWidget(0);
            Assert.That(widgetResult, Is.Null, "Still yields null on an invalid identifier");

            _contentManager.Flush();
            widgetResult = _widgetService.GetWidget(widgetPart.Id);
            Assert.That(widgetResult.Id, Is.EqualTo(widgetPart.Id), "Returns correct widget");
        }

        [Test]
        public void GetWidgetsTest() {
            const string layerName = "Test layer 1";
            const string layerDescription = "Test layer 1";
            const string widgetTitle1 = "Test widget 1";
            const string widgetTitle2 = "Test widget 2";

            LayerPart layerPart = _widgetService.CreateLayer(layerName, layerDescription, "");
            _contentManager.Flush();

            IEnumerable<WidgetPart> widgetResults = _widgetService.GetWidgets();
            Assert.That(widgetResults.Count(), Is.EqualTo(0));

            WidgetPart widgetPart = _widgetService.CreateWidget(layerPart.Id, "HtmlWidget", widgetTitle1, "1", "");
            Assert.That(widgetPart, Is.Not.Null);
            _contentManager.Flush();

            widgetResults = _widgetService.GetWidgets();
            Assert.That(widgetResults.Count(), Is.EqualTo(1));
            Assert.That(widgetResults.First().Id, Is.EqualTo(widgetPart.Id));

            _widgetService.CreateWidget(layerPart.Id, "HtmlWidget", widgetTitle2, "2", "");
            _contentManager.Flush();

            widgetResults = _widgetService.GetWidgets();
            Assert.That(widgetResults.Count(), Is.EqualTo(2));
        }

        [Test]
        public void CreateWidgetTest() {
            const string layerName = "Test layer 1";
            const string layerDescription = "Test layer 1";
            const string widgetTitle = "Test widget 1";

            LayerPart layerPart = _widgetService.CreateLayer(layerName, layerDescription, "");
            _contentManager.Flush();

            WidgetPart widgetPart = _widgetService.CreateWidget(layerPart.Id, "HtmlWidget", widgetTitle, "1", "");
            Assert.That(widgetPart, Is.Not.Null);
            Assert.That(widgetPart.LayerPart.Id, Is.EqualTo(layerPart.Id));
        }

        [Test]
        public void GetZonesTest() {
            IEnumerable<string> zones = _widgetService.GetZones();
            Assert.That(zones.Count(), Is.EqualTo(2), "One zone on the mock list");
            Assert.That(zones.FirstOrDefault(zone => zone == ThemeZoneName1), Is.Not.Null);
            Assert.That(zones.FirstOrDefault(zone => zone == ThemeZoneName2), Is.Not.Null);
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