<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BlogPart>" %>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<fieldset>
    <%: Html.LabelFor(m => m.Description) %>
    <%: Html.TextAreaFor(m => m.Description, 5, 60, null) %>
</fieldset>