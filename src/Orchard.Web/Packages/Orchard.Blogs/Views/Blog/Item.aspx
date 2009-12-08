<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<BlogViewModel>" %>

<%@ Import Namespace="Orchard.Core.Common.Models" %>
<%@ Import Namespace="Orchard.Models" %>
<%@ Import Namespace="Orchard.Blogs.Extensions" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels" %>
<%@ Import Namespace="Orchard.Blogs.Models" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels" %>
<%-- todo: (heskew) make master-less when we get into theming --%>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <%=Html.DisplayForItem(m=>m.Blog) %>
    <%--    <div class="manage"><a href="<%=Url.BlogEdit(Model.Blog.Slug) %>" class="ibutton edit">edit</a></div>
    <h2><%=Html.Encode(Model.Blog.Name) %></h2>
    <div><%=Html.Encode(Model.Blog.Description) %></div>
    <%=Html.UnorderedList(Model.Posts, (bp, i) => Html.DisplayForItem(bp).ToHtmlString(), "posts contentItems") %>
--%>
</asp:Content>
