<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Core.Contents.ViewModels.ListContentsViewModel>" %>
<%@ Import Namespace="Orchard.Core.Contents.ViewModels" %>
<h1><%:Html.TitleForPage((string.IsNullOrEmpty(Model.TypeDisplayName) ? T("Manage Content") : T("Manage {0} Content", Model.TypeDisplayName)).ToString()) %></h1>
<div class="manage">
    <%:Html.ActionLink(!string.IsNullOrEmpty(Model.TypeDisplayName) ? T("Create New {0}", Model.TypeDisplayName).Text : T("Create New Content").Text, "Create", new { }, new { @class = "button primaryAction" }) %>
</div><%
using (Html.BeginFormAntiForgeryPost()) { %>
    <fieldset class="bulk-actions">
        <label for="publishActions"><%:T("Actions:") %></label>
        <select id="publishActions" name="<%:Html.NameOf(m => m.Options.BulkAction) %>">
            <%:Html.SelectOption(Model.Options.BulkAction, ContentsBulkAction.None, T("Choose action...").ToString()) %>
            <%:Html.SelectOption(Model.Options.BulkAction, ContentsBulkAction.PublishNow, T("Publish Now").ToString()) %>
            <%:Html.SelectOption(Model.Options.BulkAction, ContentsBulkAction.Unpublish, T("Unpublish").ToString()) %>
            <%:Html.SelectOption(Model.Options.BulkAction, ContentsBulkAction.Remove, T("Remove").ToString()) %>
        </select>
        <button type="submit" name="submit.BulkEdit" value="yes"><%:T("Apply") %></button>
    </fieldset>
    <fieldset class="bulk-actions">
        <label for="filterResults" class="bulk-filter"><%:T("Show only of type")%></label>
        <select id="filterResults" name="<%:Html.NameOf(m => m.Options.SelectedFilter) %>">
            <%:Html.SelectOption(Model.Options.SelectedFilter, "", T("any (show all)").ToString()) %>
            <% foreach(var filterOption in Model.Options.FilterOptions) { %>
                <%:Html.SelectOption(Model.Options.SelectedFilter, filterOption.Key, filterOption.Value) %><%
            } %>
        </select>
        <label for="orderResults" class="bulk-order"><%:T("Ordered by")%></label>
        <select id="orderResults" name="<%:Html.NameOf(m => m.Options.OrderBy) %>">
            <%:Html.SelectOption(Model.Options.OrderBy, ContentsOrder.Created, T("most recently created").ToString())%>
            <%:Html.SelectOption(Model.Options.OrderBy, ContentsOrder.Modified, T("most recently modified").ToString())%>
            <%--<%:Html.SelectOption(Model.Options.OrderBy, ContentsOrder.Published, T("Date Published").ToString())%>--%>
        </select>
        <button type="submit" name="submit.Filter" value="yes please"><%:T("Apply") %></button>
    </fieldset>
    <fieldset class="contentItems bulk-items">
    <%:Html.UnorderedList(
        Model.Entries,
        (entry, i) => Html.DisplayForItem(entry.ViewModel),
        "") %>
    </fieldset><%
} %>