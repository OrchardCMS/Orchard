<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<IEnumerable<BlogPost>>" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<% Html.Include("Header"); %>
    <div class="yui-g">
        <h2>Posts</h2><%
        //TODO: (erikpo) Replace this with an Html extension method of some sort (ListForModel?)
        if (Model.Count() > 0) { %>
        <ul><%
            foreach (BlogPost post in Model) { %>
            <li><a href="<%=Url.BlogPost(post.BlogSlug, post.Slug) %>"><%=Html.Encode(post.Title) %></a></li><%
            } %>
        </ul><%
        } %>
    </div>
<% Html.Include("Footer"); %>