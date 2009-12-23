<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<PageEditViewModel>" %>
<%@ Import Namespace="Orchard.Sandbox.ViewModels" %>
<h1><%=Html.TitleForPage("Edit Page")%></h1>
<%using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.EditorForItem(Model.Page) %>
    <input type="submit" name="submit" value="Save" />
<% } %>
