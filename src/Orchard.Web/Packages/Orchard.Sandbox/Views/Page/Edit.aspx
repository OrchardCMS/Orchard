<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<PageEditViewModel>" %>

<%@ Import Namespace="Orchard.Sandbox.ViewModels" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<h3>
    Edit Page</h3>
<%using (Html.BeginForm()) { %>
<%=Html.EditorForItem(Model.Page) %>
<input type="submit" name="submit" value="Save" />
<%} %>
