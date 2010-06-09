<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<PageCreateViewModel>" %>
<%@ Import Namespace="Orchard.Sandbox.ViewModels" %>
<h1><%: Html.TitleForPage(T("Create Page").ToString())%></h1>
<% using (Html.BeginFormAntiForgeryPost()) { %>
    <fieldset>
        <%: Html.LabelFor(x => x.Name) %>
        <%: Html.EditorFor(x => x.Name) %>
        <input type="submit" name="submit" value="<%: T("Add") %>" />
    </fieldset>
<% } %>
