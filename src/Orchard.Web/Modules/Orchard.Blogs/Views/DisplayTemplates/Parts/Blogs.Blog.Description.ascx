<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Blog>" %>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<div class="blogdescription">
    <p><%: Model.Description %></p>
</div>