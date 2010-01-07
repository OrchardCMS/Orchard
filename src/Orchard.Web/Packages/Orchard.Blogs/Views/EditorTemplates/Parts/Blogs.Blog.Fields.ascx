<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Blog>" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<fieldset>
    <%=Html.LabelFor(m => m.Name) %>
    <%=Html.EditorFor(m => m.Name) %>
</fieldset>
<fieldset class="permalink">
    <label class="sub" for="Slug"><%=_Encoded("Permalink")%><br /><span><%=Html.Encode(Request.Url.ToRootString()) %>/</span></label>
    <span><%=Html.TextBoxFor(m => m.Slug, new { @class = "text" })%></span>
</fieldset>
<fieldset>
    <%=Html.LabelFor(m => m.Description) %>
    <%=Html.TextAreaFor(m => m.Description, 5, 60, null) %>
</fieldset>