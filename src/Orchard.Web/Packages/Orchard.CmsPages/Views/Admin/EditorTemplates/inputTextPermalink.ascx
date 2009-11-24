<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<fieldset>
    <label class="sub" for="permalink">Permalink: <span>http://localhost/</span></label>
    <span><%=Html.TextBox("", Model, new { id = "permalink", @class = "text" })%> <span class="helperText smallText clearLayout"> &laquo; How to write a permalink. &raquo; </span></span>
</fieldset>