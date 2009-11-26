<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<BlogViewModel>" %>
<%@ Import Namespace="Orchard.Core.Common.Models"%>
<%@ Import Namespace="Orchard.Models"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%-- todo: (heskew) make master-less when we get into theming --%>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <h2>Blog</h2>
    <div><%=Html.Encode(Model.Blog.Name) %></div><%
    //TODO: (erikpo) Move this into a helper
    if (Model.Posts.Count() > 0) { %>
    <ul><%
        foreach (BlogPost post in Model.Posts) { %>
        <li><a href="<%=Url.BlogPost(Model.Blog.Slug, post.As<RoutableAspect>().Slug) %>"><%=Html.Encode(post.As<RoutableAspect>().Title) %></a></li><%
        } %>
    </ul><%
    } %>
</asp:Content>