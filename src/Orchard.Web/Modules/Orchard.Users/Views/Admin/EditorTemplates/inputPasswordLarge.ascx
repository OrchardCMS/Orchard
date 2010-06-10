<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<string>" %>
<fieldset>
    <%: Html.LabelForModel() %>
    <%: Html.Password("", Model, new { @class = "textMedium" })%>
    <%: Html.ValidationMessage("", "*") %>
</fieldset>