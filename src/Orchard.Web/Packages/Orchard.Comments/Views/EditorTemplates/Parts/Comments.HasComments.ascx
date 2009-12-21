<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<HasComments>" %>
<%@ Import Namespace="Orchard.Comments.Models"%>
<fieldset>
    <legend>Comments<% if (Model.ContentItem.Id != 0) { %>: <% var commentCount = Model.Comments.Count(); %>
    <%=Html.ActionLink(
            string.Format("{0} comment{1}", commentCount, commentCount == 1 ? "" : "s"),
            "Details",
            new { Area = "Orchard.Comments", Controller = "Admin", id = Model.ContentItem.Id, returnUrl = Context.Request.Url }
            ) %>
    - <a href="#">0 pending</a><% } %></legend>
    <label for="Closed">
    <% if (Model.Closed) {%>
        <input id="Closed" name="Closed" type="checkbox" checked="checked" />
     <% } else { %>
        <input id="Closed" name="Closed" type="checkbox" />
     <% } %> 
        Comments are disabled
     </label>
</fieldset>

