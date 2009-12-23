<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<HasComments>" %>
<%@ Import Namespace="Orchard.Comments.Models"%>
<span class="commentcount"><a href="#comments"><%=Model.Comments.Count() %> Comment<%=Model.Comments.Count() == 1 ? "" : "s" %></a></span>
