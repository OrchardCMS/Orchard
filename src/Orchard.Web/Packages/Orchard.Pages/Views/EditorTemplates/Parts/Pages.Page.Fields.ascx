<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Orchard.Pages.Models.Page>" %>
<%@ Import Namespace="Orchard.Pages.Extensions"%>
<fieldset>
    <label for="Title">Title</label>
    <span><%=Html.TextBoxFor(m => m.Title, new { @class = "large text" })%></span>
</fieldset>
<fieldset class="permalink">
    <label class="sub" for="Slug">Permalink<br /><span><%=Request.Url.ToRootString() %>/</span></label>
    <span><%=Html.TextBoxFor(m => m.Slug, new { @class = "text" })%></span>
</fieldset>