<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<HasComments>" %>
<%@ Import Namespace="Orchard.Comments.Models"%>
<span class="commentcount"><%=_Encoded("{0} Comment{1}", Model.CommentCount, Model.CommentCount == 1 ? "" : "s")%></span>