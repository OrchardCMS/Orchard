<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<EditTagsViewModel>" %>
<%@ Import Namespace="Orchard.Tags.ViewModels" %>
<%@ Import Namespace="Orchard.Tags.Models" %>
<fieldset>
    <%=Html.LabelFor(m=>m.Tags) %>
    <%=Html.EditorFor(m=>m.Tags) %>
</fieldset>
<%--<input class="large text" id="Tags" name="Tags" type="text" value="<%=Model.Tags %>" />--%>
