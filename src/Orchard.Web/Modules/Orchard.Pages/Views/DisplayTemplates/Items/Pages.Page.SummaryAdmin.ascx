<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<Orchard.Pages.Models.Page>>" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels" %>
    <div class="summary">
        <div class="properties">
            <h3><%:Html.ActionLink(Model.Item.Title, "Edit", new { id = Model.Item.Id }) %></h3><%
            Html.Zone("metadata"); %>
        </div>
        <div class="related"><%
            if (Model.Item.HasPublished) { %>
            <%:Html.ActionLink("View", "Item", new { controller = "Page", slug = Model.Item.PublishedSlug }, new { title = T("View Page") }) %><%:T(" | ") %><%
                if (Model.Item.HasDraft) { %>
            <a href="<%:Html.AntiForgeryTokenGetUrl(Url.Action("Publish", new {id = Model.Item.Id})) %>" title="<%:T("Publish Draft") %>"><%:T("Publish Draft") %></a><%:T(" | ") %><%
                } %>
            <a href="<%:Html.AntiForgeryTokenGetUrl(Url.Action("Unpublish", new {id = Model.Item.Id})) %>" title="<%:T("Unpublish Page") %>"><%:T("Unpublish") %></a><%:T(" | ") %><%
            }
            else { %>
            <a href="<%:Html.AntiForgeryTokenGetUrl(Url.Action("Publish", new {id = Model.Item.Id})) %>" title="<%:T("Publish Page")%>"><%:T("Publish") %></a><%:T(" | ") %><%
            } %>
            <%:Html.ActionLink(T("Edit").ToString(), "Edit", new { id = Model.Item.Id }, new { title = T("Edit Page").ToString() })%><%:T(" | ") %>
            <a href="<%:Html.AntiForgeryTokenGetUrl(Url.Action("Delete", new {id = Model.Item.Id})) %>" title="<%:T("Remove Page") %>"><%:T("Remove") %></a>
        </div>
        <div style="clear:both;"></div>
        <% Html.Zone("primary"); %>
    </div>