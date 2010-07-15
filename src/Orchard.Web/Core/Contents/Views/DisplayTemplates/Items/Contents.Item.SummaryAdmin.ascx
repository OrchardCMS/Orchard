<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels" %>
    <div class="summary">
        <div class="properties">
            <h3><%:Html.ItemEditLink(Model.Item) %></h3>
            <div class="metadata"><% Html.Zone("metadata"); %></div>
        </div>
        <div class="related"><%
            Html.Zone("secondary"); %>
            <%:Html.ItemEditLink(T("Edit").Text, Model.Item) %><%:T(" | ") %><%
            using (Html.BeginFormAntiForgeryPost(string.Format("{0}", Url.Action("Remove", new { area = "Contents" })), FormMethod.Post, new {@class = "inline link"})) { %>
                <%:Html.Hidden("id", Model.Item.Id, new { id = "" })%>
                <button type="submit"><%:T("Remove") %></button><%
            } %>
        </div>
        <div style="clear:both;"></div>
        <% Html.Zone("primary"); %>
    </div>