<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BlogPart>" %>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<div class="blog-description">
    <p><%: Model.Description %></p>
</div>