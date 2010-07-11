<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<EditTagsViewModel>" %>
<%@ Import Namespace="Orchard.Tags.ViewModels"%>
<fieldset>
    <%: Html.LabelFor(m => m.Tags) %>
    <%: Html.TextBoxFor(m => m.Tags, new { @class = "large text" })%>
</fieldset>