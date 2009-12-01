<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="ViewPage<BlogPostViewModel>" %>
<%@ Import Namespace="Orchard.Core.Common.Models"%>
<%@ Import Namespace="Orchard.Models"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%-- todo: (heskew) make master-less when we get into theming --%>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <h1><%=Html.Encode(Model.Post.Title) %></h1>
    <div class="metadata">
        <div class="posted">Posted by <%=Html.Encode(Model.Post.Creator.UserName) %> <%=Model.Post.Published.HasValue ? "on" : "as a" %> <%=Html.Published(Model.Post) %></div>
        <div><a href="<%=Url.BlogPostEdit(Model.Blog.Slug, Model.Post.Slug) %>">(edit)</a></div>
    </div>
    <div class="content"><%=Model.Post.Body %></div>
    
    <%foreach (var display in Model.Displays) { %>
            <%=Html.DisplayFor(m=>display.Model, display.TemplateName, display.Prefix) %>
    <%} %>
</asp:Content>