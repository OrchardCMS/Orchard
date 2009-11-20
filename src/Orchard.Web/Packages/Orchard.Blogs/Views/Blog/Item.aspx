<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Blog>" %>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<% Html.Include("Header"); %>
    <div class="yui-g">
        <h2>Blog</h2>
        <div><%=Html.Encode(Model.Slug) %></div>
    </div>
<% Html.Include("Footer"); %>