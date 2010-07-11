<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<CommentCountViewModel>" %>
<%@ Import Namespace="Orchard.Comments.ViewModels"%>
<span class="commentcount"><%: T.Plural("1 Comment", "{0} Comments", Model.CommentCount)%></span>
