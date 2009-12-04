<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<TagsSearchViewModel>" %>
<%@ Import Namespace="Orchard.Models"%>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%-- todo: (heskew) make master-less when we get into theming --%>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>List of contents tagged with <%= Model.TagName %></h2><%
    foreach (var contentItem in Model.Contents) { %>
    <%=Html.ItemDisplayLink(contentItem)%>&nbsp;
    <% } %>
</asp:Content>