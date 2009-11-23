<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.Blogs.ViewModels.CreateBlogViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Security" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("Head"); %>
    <div class="yui-u">
        <h2 class="separator">
            Create New Blog</h2>
        <p class="bottomSpacer">
        <a href="<%=Url.Blogs() %>">Manage Blogs</a> > Create
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