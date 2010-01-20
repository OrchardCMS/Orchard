<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<HasComments>" %>
<%@ Import Namespace="Orchard.Comments.Models"%>
<h2 id="comments"><%=_Encoded("{0} Comment{1}", Model.CommentCount, Model.CommentCount == 1 ? "" : "s") %></h2><%
if (Model.CommentCount > 0) { Html.RenderPartial("ListOfComments", Model.Comments); }
if (Model.CommentsActive == false) { %>
    <p><%=_Encoded("Comments have been disabled for this content.") %></p><%
} else { %>
    <%-- todo: (heskew) need a comment form for the authenticated user... --%>
    <% using(Html.BeginForm("Create", "Admin", new { area = "Orchard.Comments" }, FormMethod.Post, new { @class = "comment" })) { %>
        <%=Html.ValidationSummary() %>
        <h2>Add a Comment</h2>
        <fieldset class="who">

            <%=Html.Hidden("CommentedOn", Model.ContentItem.Id) %>
            <%=Html.Hidden("ReturnUrl", Context.Request.Url) %>
            <div>
                <label for="Name"><%=_Encoded("Name") %></label>
                <input id="Name" class="text" name="Name" type="text" />
            </div>
            <div>
                <label for="Email"><%=_Encoded("Email") %></label>
                <input id="Email" class="text" name="Email" type="text" />				
            </div>
            <div>
                <label for="SiteName"><%=_Encoded("Url") %></label>
                <input id="SiteName" class="text" name="SiteName" type="text" />
            </div>

            <div>
                <label for="CommentText"><%=_Encoded("Comment") %></label>
                <textarea id="CommentText" rows="10" cols="30" name="CommentText"></textarea>
            </div>
       
            <div>
                <input type="submit" class="button" value="<%=_Encoded("Submit Comment") %>" />
		        <%=Html.AntiForgeryTokenOrchard() %>
            </div>
        </fieldset><%
   }
} %>

    