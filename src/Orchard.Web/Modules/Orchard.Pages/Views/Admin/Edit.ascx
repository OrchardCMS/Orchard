<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<PageEditViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Pages.ViewModels"%>
<h1><%=Html.TitleForPage(T("Edit Page").ToString()) %></h1>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
    <%=Html.EditorForItem(m => m.Page) %>
    <fieldset>
        <%=Html.EditorFor(m => m.PromoteToHomePage) %>
        <label for="PromoteToHomePage" class="forcheckbox"><%=_Encoded("Set as home page") %></label>
    </fieldset><%
   } %>