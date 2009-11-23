<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<string>" %>
<li>
    <%=Html.LabelForModel() %>
    <%=Html.TextBox("",Model,new{@class="text large"}) %>
    <%=Html.ValidationMessage("","*")%>
</li>
