<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogEditViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<h2><%=Html.TitleForPage("Edit Blog") %></h2>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
    <%=Html.EditorForItem(m => m.Blog) %>
    <fieldset><input class="button" type="submit" value="Save" /></fieldset><%
   } %>