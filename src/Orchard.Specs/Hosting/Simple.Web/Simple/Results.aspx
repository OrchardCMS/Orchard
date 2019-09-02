<%@ Page %>

<ul>
<li>passthrough1:<%=Server.HtmlEncode(Request.Form.Get("passthrough1")) %></li>
<li>passthrough2:<%=Server.HtmlEncode(Request.Form.Get("passthrough2")) %></li>
<li>input1:<%=Server.HtmlEncode(Request.Form.Get("input1")) %></li>
</ul>
