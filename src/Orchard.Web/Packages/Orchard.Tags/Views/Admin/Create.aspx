<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<TagsAdminCreateViewModel>" %>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<h1><%=Html.TitleForPage(T("Add a Tag").ToString()) %></h1>
<% using(Html.BeginFormAntiForgeryPost()) { %>
    <%=Html.ValidationSummary() %>
    <fieldset>
        <label for="TagName"><%=_Encoded("Name:")%></label>
	    <input id="TagName" class="text" name="TagName" type="text" value="<%=Html.Encode(Model.TagName) %>" />
        <input type="submit" class="button" value="<%=_Encoded("Save") %>" />
    </fieldset>
<% } %>