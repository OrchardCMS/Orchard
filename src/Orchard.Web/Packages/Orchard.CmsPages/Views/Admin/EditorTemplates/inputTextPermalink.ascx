<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<li>
<label class="floatLeft" for="permalink">
Permalink:
<span>http://localhost/</span>
</label>
<span class="floatLeft">
<%--<input id="permalink" class="inputText floatLeft roundCorners" type="text" name="permalink"/>--%>
<%=Html.TextBox("", Model, new { @class = "text" })%> <span class="helperText smallText clearLayout"> &laquo; How to write a permalink. &raquo; </span>
</span>
</li>
<div class="clearLayout" />