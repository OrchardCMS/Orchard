<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<PageCreateViewModel>" %>
<%@ Import Namespace="Orchard.Sandbox.ViewModels" %>
<h1><%=Html.TitleForPage("Create Page")%></h1>
<%using (Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.LabelFor(x => x.Name)%><%=Html.EditorFor(x => x.Name)%>
    <input type="submit" name="submit" value="Create" />
<% } %>
