<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Core.Common.ViewModels.CommonMetadataViewModel>" %>
<%  // todo: make this all work
    if (Model.HasPublished) { %>
    <%:Html.ItemDisplayLink(T("View").Text, Model.ContentItem) %><%:T(" | ") %><%
        if (Model.HasDraft) { %>
    <a href="<%:Html.AntiForgeryTokenGetUrl(Url.Action("Publish", new {id = Model.ContentItem.Id})) %>" title="<%:T("Publish Draft") %>"><%:T("Publish Draft") %></a><%:T(" | ") %><%
        } %>
    <a href="<%:Html.AntiForgeryTokenGetUrl(Url.Action("Unpublish", new {id = Model.ContentItem.Id})) %>" title="<%:T("Unpublish") %>"><%:T("Unpublish") %></a><%:T(" | ") %><%
    }
    else { %>
    <a href="<%:Html.AntiForgeryTokenGetUrl(Url.Action("Publish", new {id = Model.ContentItem.Id})) %>" title="<%:T("Publish")%>"><%:T("Publish") %></a><%:T(" | ") %><%
    } %>