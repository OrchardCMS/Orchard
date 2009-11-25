<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogPostViewModel>" %>
<%@ Import Namespace="Orchard.Core.Common.Models"%>
<%@ Import Namespace="Orchard.Models"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<% Html.Include("Header"); %>
    <div class="yui-g">
        <h2><%=Html.Encode(Model.Post.Title) %></h2>
        <div>Posted by <%=Html.Encode(Model.Post.Creator.UserName) %> on {M d yyyy h:mm tt}</div>
        <%=Model.Post.Body %>
    </div>
<% Html.Include("Footer"); %>