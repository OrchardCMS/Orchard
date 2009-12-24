<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<HasComments>" %>
<%@ Import Namespace="Orchard.Comments.Models"%>
<span class="commentcount"><a href="#comments"><%=Model.CommentCount %> Comment<%=Model.CommentCount == 1 ? "" : "s" %></a></span>
