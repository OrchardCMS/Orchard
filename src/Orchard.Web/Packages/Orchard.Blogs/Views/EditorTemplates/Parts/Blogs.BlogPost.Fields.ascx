<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BlogPost>" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<fieldset>
    <%=Html.LabelFor(m => m.Title) %>
    <%=Html.TextBoxFor(m => m.Title, new { @class = "large text" })%>
</fieldset>
<fieldset class="permalink">
    <label class="sub" for="Slug"><%=_Encoded("Permalink")%><br /><span><%=Html.Encode(Request.Url.ToRootString()) %>/<%=Html.Encode(Model.Blog.Slug) %>/</span></label>
    <span><%=Html.TextBoxFor(m => m.Slug, new { @class = "text" })%></span>
</fieldset>