<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<fieldset>
    <label class="sub" for="permalink">Permalink: <span><%=Request.Url.ToRootString() %>/</span></label>
    <span><%=Html.TextBox("", Model, new { id = "permalink", @class = "text" })%> <span> &laquo; How to write a permalink. &raquo; </span></span>
</fieldset>