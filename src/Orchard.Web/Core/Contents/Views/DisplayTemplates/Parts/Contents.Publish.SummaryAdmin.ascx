<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Core.Contents.ViewModels.PublishContentViewModel>" %>
<%  // todo: make this all work
    if (Model.HasPublished) { %>
    <%:Html.ItemDisplayLink(T("View").Text, Model.ContentItem) %><%:T(" | ") %><%
        if (Model.HasDraft) { %>
    <%:Html.Link(T("Publish Draft").Text, Url.Action("Publish", new { area = "Contents", id = Model.ContentItem.Id }), new { itemprop = "PublishUrl UnsafeUrl" })%><%:T(" | ") %><%
        } %>
    <%:Html.Link(T("Unpublish").Text, Url.Action("Unpublish", new { area = "Contents", id = Model.ContentItem.Id }), new { itemprop = "UnpublishUrl UnsafeUrl" })%><%:T(" | ") %><%
    }
    else { %>
    <%:Html.Link(T("Publish").Text, Url.Action("Publish", new { area = "Contents", id = Model.ContentItem.Id }), new { itemprop = "PublishUrl UnsafeUrl" })%><%:T(" | ") %><%
    } %>