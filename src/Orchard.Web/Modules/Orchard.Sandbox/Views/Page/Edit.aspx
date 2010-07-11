<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<PageEditViewModel>" %>
<%@ Import Namespace="Orchard.Sandbox.ViewModels" %>
<h1><%: Html.TitleForPage(T("Edit Page").ToString()) %></h1>
<%using (Html.BeginFormAntiForgeryPost()) { %>
    <%: Html.EditorForItem(Model.Page) %>
    <fieldset>
        <input type="submit" name="submit" value="<%: T("Save") %>" />
    </fieldset>
<% } %>
