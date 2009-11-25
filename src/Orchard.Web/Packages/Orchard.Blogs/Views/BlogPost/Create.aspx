<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<CreateBlogPostViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Security" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("Head"); %>
    <div class="yui-u">
        <h2 class="separator">
            Create a New Blog Post</h2>
        <p class="bottomSpacer">
        <a href="<%=Url.Blogs() %>">Manage Blogs</a> > <a href="<%=Url.BlogEdit(Model.Blog.Slug) %>"><%=Html.Encode(Model.Blog.Name) %></a> > Create Blog Post
        </p>
    </div>
    <div class="yui-u">
        <%using (Html.BeginForm()) { %>
        <ol>
            <%= Html.ValidationSummary() %>
            <%= Html.EditorForModel() %>
            <li class="clearLayout">
                <input class="button" type="submit" value="Create" />
                <a href="<%=Url.Blogs() %>" class="button">Cancel</a>
                </li>
        </ol>
        <%}/*EndForm*/%>
    </div>
<% Html.Include("Foot"); %>