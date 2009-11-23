<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEditViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<%@ Import Namespace="Orchard.Security" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.Include("Header"); %>
    <div class="yui-u">
        <h2 class="separator">
            Edit Blog</h2>
        <p class="bottomSpacer">
        <%=Html.ActionLink("Blogs", "List", "Blog") %> > Edit #<%= Model.Id%> <strong><%=Html.Encode(Model.Name)%></strong>
        </p>
    </div>
    <div class="yui-u">
        <%using (Html.BeginForm()) { %>
        <ol>
            <%= Html.ValidationSummary() %>
            <%= Html.EditorForModel() %>
            <li class="clearLayout">
                <input class="button" type="submit" value="Save" /> 
                <%=Html.ActionLink("Cancel", "Index", new{}, new{@class="button"}) %>
                </li>
        </ol>
        <%}/*EndForm*/%>
    </div>
<% Html.Include("Footer"); %>