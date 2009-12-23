<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<PageEditViewModel>" %>
<%@ Import Namespace="Orchard.Sandbox.ViewModels" %>
<h3>
    Edit Page</h3>
<%using (Html.BeginFormAntiForgeryPost()) { %>
<%=Html.EditorForItem(Model.Page) %>
<input type="submit" name="submit" value="Save" />
<%} %>
