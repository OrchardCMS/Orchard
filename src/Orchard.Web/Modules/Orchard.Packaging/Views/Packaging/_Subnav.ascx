<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<p>
    <%:Html.ActionLink("Browse Repository Packages", "Index") %>
    &bull;
    <%:Html.ActionLink("Harvest Local Packages", "Harvest") %>
    &bull;
    <%:Html.ActionLink("Edit Repository Sources", "Sources") %>
</p>
