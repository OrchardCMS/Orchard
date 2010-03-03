<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BlogEditViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<h1><%=Html.TitleForPage(T("Edit Blog").ToString()) %></h1>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
    <%=Html.EditorForItem(m => m.Blog) %>
    <%=Html.EditorFor(m => m.PromoteToHomePage) %>
    <label for="PromoteToHomePage" class="forcheckbox"><%=_Encoded("Set as home page") %></label>
    <fieldset><input class="button primaryAction" type="submit" value="<%=_Encoded("Save") %>" /></fieldset><%
   } %>