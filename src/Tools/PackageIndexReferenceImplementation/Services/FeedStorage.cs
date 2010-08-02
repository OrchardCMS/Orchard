using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web;
using System.Web.Hosting;
using System.Xml;

namespace PackageIndexReferenceImplementation.Services {
    public class FeedStorage {

        public SyndicationFeed GetFeed() {
            var formatter = new Atom10FeedFormatter<SyndicationFeed>();
            var feedPath = HostingEnvironment.MapPath("~/App_Data/Feed.xml");
            if (!File.Exists(feedPath)) {
                string title = ConfigurationManager.AppSettings["Title"];
                return new SyndicationFeed() { Title = new TextSyndicationContent(title)};
            }
            using (var reader = XmlReader.Create(feedPath)) {
                formatter.ReadFrom(reader);
                return formatter.Feed;
            }
        }

        public void StoreFeed(SyndicationFeed feed) {
            var formatter = new Atom10FeedFormatter<SyndicationFeed>(feed);
            var feedPath = HostingEnvironment.MapPath("~/App_Data/Feed.xml");
            using (var writer = XmlWriter.Create(feedPath)) {
                formatter.WriteTo(writer);
            }
        }
    }
}