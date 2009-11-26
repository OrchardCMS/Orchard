<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<TagsIndexViewModel>" %>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%-- todo: (heskew) make master-less when we get into theming --%>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <h2>Tags</h2>
    <%=Html.ValidationSummary() %>
    <% foreach (var tag in Model.Tags) { %>
        <%=Html.ActionLink(tag.TagName, "Search", new {tagName = tag.TagName}, new {@class="floatRight topSpacer"}) %>
        &nbsp;
    <% } %>
</asp:Content>