<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CreateBlogViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<h2><%=Html.TitleForPage("Add Blog") %></h2>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
    <%=Html.EditorForItem(vm => vm.Blog) %>
    <fieldset><input class="button" type="submit" value="Create" /></fieldset><%
   } %>