<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<% Html.Include("Header"); %>
    <div class="yui-g">
        <h2>Blog</h2>
        <div><%=Html.Encode(Model.Blog.Name) %></div><%
        //TODO: (erikpo) Move this into a helper
        if (Model.Posts.Count() > 0) { %>
        <ul><%
            foreach (BlogPost post in Model.Posts) { %>
            <li><%=post.Title %></li><%
            } %>
        </ul><%
        } %>
    </div>
<% Html.Include("Footer"); %>