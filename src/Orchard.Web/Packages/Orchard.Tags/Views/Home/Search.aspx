<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<TagsSearchViewModel>" %>
<%@ Import Namespace="Orchard.Models"%>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%-- todo: (heskew) make master-less when we get into theming --%>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>List of contents tagged with <em><%= Model.TagName %></em></h2>
    <%=Html.UnorderedList(Model.Items, (c, i) => Html.DisplayForItem(x=>c).ToHtmlString(), "contentItems") %>
</asp:Content>