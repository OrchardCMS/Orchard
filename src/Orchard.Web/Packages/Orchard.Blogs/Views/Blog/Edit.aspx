<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEditViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<%@ Import Namespace="Orchard.Security" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("Head"); %>
    <div class="yui-u">
        <h2 class="separator">
            Edit Blog</h2>
        <p class="bottomSpacer">
        <a href="<%=Url.Blogs() %>">Manage Blogs</a> > Edit #<%= Model.Id%> <strong><%=Html.Encode(Model.Name)%></strong>
        </p>
    </div>
    <div class="yui-u">
        <%using (Html.BeginForm()) { %>
        <ol>
            <%= Html.ValidationSummary() %>
            <%= Html.EditorForModel() %>
            <li class="clearLayout">
                <input class="button" type="submit" value="Save" /> 
                <a href="<%=Url.Blogs() %>" class="button">Cancel</a>
                </li>
        </ol>
        <%}/*EndForm*/%>
    </div>
<% Html.Include("Foot"); %>