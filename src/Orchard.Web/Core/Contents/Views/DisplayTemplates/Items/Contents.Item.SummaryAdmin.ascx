<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels" %>
    <div class="summary" itemscope="itemscope" itemid="<%:Model.Item.Id %>" itemtype="http://orchardproject.net/data/ContentItem">
        <div class="properties">
            <input type="checkbox" value="<%:Model.Item.Id %>" name="itemIds"/>
            <h3><%:Html.ItemEditLink(Model.Item) %></h3>
            <div class="metadata"><% Html.Zone("metadata"); %></div>
        </div>
        <div class="related"><%
            Html.Zone("secondary"); %>
            <%:Html.ItemEditLink(T("Edit").Text, Model.Item) %><%:T(" | ") %>
            <%:Html.Link(T("Remove").Text, Url.Action("Remove", new { area = "Contents", id = Model.Item.Id }), new { itemprop = "RemoveUrl UnsafeUrl" }) %>
            <br /><% Html.Zone("meta"); %>
        </div>
        <div class="primary"><% Html.ZonesAny(); %></div>
    </div>