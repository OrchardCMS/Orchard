<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<CreateBlogViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<h1><%: Html.TitleForPage(T("Add Blog").ToString()) %></h1>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%: Html.ValidationSummary() %>
    <%: Html.EditorForItem(vm => vm.Blog) %>
    <fieldset>
        <%: Html.EditorFor(m => m.PromoteToHomePage) %>
        <label for="PromoteToHomePage" class="forcheckbox"><%: T("Set as home page") %></label>
    </fieldset>
    <fieldset><input class="button primaryAction" type="submit" value="<%: T("Add") %>" /></fieldset><%
   } %>