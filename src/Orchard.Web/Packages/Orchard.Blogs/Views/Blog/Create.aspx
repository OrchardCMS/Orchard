<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Orchard.Blogs.ViewModels.CreateBlogViewModel>" %>
<%@ Import Namespace="Orchard.Security" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("Header"); %>
    <div class="yui-u">
        <h2 class="separator">
            Add a new Blog</h2>
            
    </div>
    <div class="yui-u">
        <%using (Html.BeginForm()) { %>
        <ol>
            <%= Html.ValidationSummary() %>
            <%= Html.EditorForModel() %>
            <li class="clearLayout">
                <input class="button" type="submit" value="Create" />
                <%=Html.ActionLink("Cancel", "Index", new{}, new{@class="button"}) %></li>
        </ol>
        <%}/*EndForm*/%>
    </div>
<% Html.Include("Footer"); %>