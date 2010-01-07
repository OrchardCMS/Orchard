<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<CreateBlogViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<h1><%=Html.TitleForPage(T("Add Blog").ToString()) %></h1>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
    <%=Html.EditorForItem(vm => vm.Blog) %>
    <fieldset><input class="button" type="submit" value="<%=_Encoded("Create") %>" /></fieldset><%
   } %>