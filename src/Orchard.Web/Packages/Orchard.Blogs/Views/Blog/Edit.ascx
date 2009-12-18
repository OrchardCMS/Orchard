<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BlogEditViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<% Html.Title("Edit Blog"); %>
<h2>Edit Blog</h2>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
    <%=Html.EditorForItem(m => m.Blog) %>
    <fieldset><input class="button" type="submit" value="Save" /></fieldset><%
   } %>