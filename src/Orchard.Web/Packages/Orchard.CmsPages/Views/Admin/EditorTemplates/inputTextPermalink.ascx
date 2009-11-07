<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<li>
<label class="floatLeft" for="permalink">
Permalink:
<span>http://localhost/</span>
</label>
<span class="floatLeft">
<%--<input id="permalink" class="inputText floatLeft roundCorners" type="text" name="permalink"/>--%>
<%=Html.TextBox("", Model, new { @class = "inputText floatLeft" })%>
<p class="helperText smallText clearLayout">How to write a permalink.</p>
</span>
</li>
<div class="clearLayout" />