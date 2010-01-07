<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BlogEditViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<h1><%=Html.TitleForPage(T("Edit Blog").ToString()) %></h1>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
    <%=Html.EditorForItem(m => m.Blog) %>
    <fieldset><input class="button" type="submit" value="<%=_Encoded("Save") %>" /></fieldset><%
   } %>