using System;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Feeds.Models;

namespace Orchard.Core.Feeds {
    public interface IFeedFormatter {
        ActionResult Process(FeedContext context, Action populate);

        FeedItem AddItem(FeedContext context, ContentItem contentItem);
        void AddProperty(FeedContext context, FeedItem feedItem, string name, string value);
    }
}