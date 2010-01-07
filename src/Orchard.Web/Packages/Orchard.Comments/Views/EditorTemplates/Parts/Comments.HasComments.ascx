<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<HasComments>" %>
<%@ Import Namespace="Orchard.Comments.Models" %>
<fieldset>
    <%-- todo: (heskew) pull the legend and put the link to the comments elsewhere? --%>
    <legend><%=Model.ContentItem.Id != 0
                ? T("Comments {0} - <a href=\"#\">?? pending</a>", Html.ActionLink(
                                        T("{0} comment{1}", Model.CommentCount, Model.CommentCount == 1 ? "" : "s").ToString(),
                                        "Details",
                                        new { Area = "Orchard.Comments", Controller = "Admin", id = Model.ContentItem.Id, returnUrl = Context.Request.Url }))
                : T("Comments")%></legend>
<%--
    todo: (heskew) can get into a weird state if this is disabled but comments are active so, yeah, comment settings on a content item need to be hashed out
    <%=Html.EditorFor(m => m.CommentsShown) %>
    <label class="forcheckbox" for="CommentsShown"><%=T("Comments are shown. Existing comments are displayed.") %></label>
--%>
    <%=Html.EditorFor(m => m.CommentsActive) %>
    <label class="forcheckbox" for="CommentsActive"><%=T("Allow new comments") %></label>
    <span class="hint forcheckbox"><%=T("Enable to show the comment form. Disabling still allows the existing comments to be shown but does not allow the conversation to continue.")%></span>
</fieldset>
