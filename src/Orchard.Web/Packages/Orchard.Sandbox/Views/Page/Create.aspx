<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<PageCreateViewModel>" %>

<%@ Import Namespace="Orchard.Sandbox.ViewModels" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<h3>
    Create Page</h3>
<%using (Html.BeginForm()) { %>

    <%=Html.LabelFor(x => x.Name)%><%=Html.EditorFor(x => x.Name)%>
    <input type="submit" name="submit" value="Create" />
<%} %>
