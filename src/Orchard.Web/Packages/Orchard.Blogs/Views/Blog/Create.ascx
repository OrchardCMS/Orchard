<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CreateBlogViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<% Html.Title("Add Blog"); %>
<h2>Add Blog</h2>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
    <%=Html.EditorForItem(vm => vm.Blog) %>
    <fieldset><input class="button" type="submit" value="Create" /></fieldset><%
   } %>