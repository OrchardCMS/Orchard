using System;
using System.Web.Mvc;
using Orchard.Core.Feeds.Models;

namespace Orchard.Core.Feeds {
    public interface IFeedBuilder {
        ActionResult Process(FeedContext context, Action populate);

        FeedItem<TItem> AddItem<TItem>(FeedContext context, TItem contentItem);
        void AddProperty(FeedContext context, FeedItem feedItem, string name, string value);
    }
}