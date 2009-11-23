<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogsViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<% Html.Include("Head"); %>
    <div class="yui-g">
        <h2>Manage Blogs</h2><%
        //TODO: (erikpo) Replace this with an Html extension method of some sort (ListForModel?)
        if (Model.Blogs.Count() > 0) { %>
        <ul><%
            foreach (Blog blog in Model.Blogs) { %>
            <li><a href="<%=Url.Blog(blog.Slug) %>"><%=Html.Encode(blog.Name) %></a> (<a href="<%=Url.BlogEdit(blog.Slug) %>">edit</a>)</li><%
            } %>
        </ul><%
        } %>
    </div>
<% Html.Include("Foot"); %>