<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<HasTags>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Tags.Models"%>
<h3>Tags</h3>
<formset>
    <label for="Tags">Tag names:</label>
    <input class="large text" id="Tags" name="Tags" type="text" value="<%=string.Join(", ", Model.CurrentTags.Select((t, i) => t.TagName).ToArray()) %>" />
</formset>
<%--<% Html.BeginForm("Update", "Home", new { area = "Orchard.Tags" }); %>--%>