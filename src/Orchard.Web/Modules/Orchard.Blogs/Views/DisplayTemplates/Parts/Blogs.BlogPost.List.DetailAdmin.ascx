<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ListContentsViewModel>" %>
<%@ Import Namespace="Orchard.Core.Contents.ViewModels" %>
<%@ Import Namespace="Orchard.Utility.Extensions" %>
<%
if (Model.Entries.Count() < 1) { %>
<div class="info message"><%:T("There are no posts for this blog.") %></div><%
}
else {
    using (Html.BeginFormAntiForgeryPost(Url.Action("List", "Admin", new { area = "Contents", id = "" }))) { %>
    <fieldset class="bulk-actions">
        <label for="publishActions"><%:T("Actions:") %></label>
        <select id="publishActions" name="<%:Html.NameOf(m => m.Options.BulkAction) %>">
            <%:Html.SelectOption(Model.Options.BulkAction, ContentsBulkAction.None, T("Choose action...").ToString()) %>
            <%:Html.SelectOption(Model.Options.BulkAction, ContentsBulkAction.PublishNow, T("Publish Now").ToString()) %>
            <%:Html.SelectOption(Model.Options.BulkAction, ContentsBulkAction.Unpublish, T("Unpublish").ToString()) %>
            <%:Html.SelectOption(Model.Options.BulkAction, ContentsBulkAction.Remove, T("Remove").ToString()) %>
        </select>
        <%:Html.Hidden("returnUrl", ViewContext.RequestContext.HttpContext.Request.ToUrlString()) %>
        <button type="submit" name="submit.BulkEdit" value="yes"><%:T("Apply") %></button>
    </fieldset>
    <fieldset class="contentItems bulk-items">
        <%:Html.UnorderedList(
            Model.Entries,
            (entry, i) => Html.DisplayForItem(entry.ViewModel),
            "") %>
    </fieldset><%
    }
} %>