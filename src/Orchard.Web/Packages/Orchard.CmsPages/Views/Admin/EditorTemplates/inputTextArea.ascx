<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<string>" %>
<li>
    <%=Html.LabelForModel() %>
    <%=Html.TextArea("",Model) %>
    <%=Html.ValidationMessage("","*") %>
</li>
