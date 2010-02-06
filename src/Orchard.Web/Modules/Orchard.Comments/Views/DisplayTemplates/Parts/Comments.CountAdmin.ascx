<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<CommentCountViewModel>" %>
<%@ Import Namespace="Orchard.Comments.Extensions"%>
<%@ Import Namespace="Orchard.Comments.ViewModels"%>
<span class="commentcount"><%=Html.CommentSummaryLinks(T, Model.Item, Model.CommentCount, Model.PendingCount)%></span>
