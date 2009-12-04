<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<HasTags>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Tags.Models"%>
<formset>
    <label for="Tags">Tags</label>
    <input class="large text" id="Tags" name="Tags" type="text" value="<%=string.Join(", ", Model.CurrentTags.Select((t, i) => t.TagName).ToArray()) %>" />
</formset>
<%--<% Html.BeginForm("Update", "Home", new { area = "Orchard.Tags" }); %>--%>