<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Orchard.Comments.Models.CommentSettingsRecord>" %>
<h3>Comments</h3>
<ol>
    <li>
        <%= Html.LabelFor(x=>x.RequireLoginToAddComment) %>
        <%= Html.EditorFor(x=>x.RequireLoginToAddComment) %>
        <%= Html.ValidationMessage("RequireLoginToAddComment", "*")%>
    </li>
    <li>
        <%= Html.LabelFor(x=>x.EnableCommentsOnPages) %>
        <%= Html.EditorFor(x=>x.EnableCommentsOnPages) %>
        <%= Html.ValidationMessage("EnableCommentsOnPages", "*")%>
    </li>
    <li>
        <%= Html.LabelFor(x=>x.EnableCommentsOnPosts) %>
        <%= Html.EditorFor(x=>x.EnableCommentsOnPosts) %>
        <%= Html.ValidationMessage("EnableCommentsOnPosts", "*")%>
    </li>
    <li>
        <%= Html.LabelFor(x=>x.EnableSpamProtection) %>
        <%= Html.EditorFor(x=>x.EnableSpamProtection) %>
        <%= Html.ValidationMessage("EnableSpamProtection", "*")%>
    </li>
     <li>
        <%= Html.LabelFor(x=>x.AkismetKey) %>
        <%= Html.EditorFor(x=>x.AkismetKey) %>
        <%= Html.ValidationMessage("AkismetKey", "*")%>
    </li>
     <li>
        <%= Html.LabelFor(x=>x.AkismetUrl) %>
        <%= Html.EditorFor(x=>x.AkismetUrl) %>
        <%= Html.ValidationMessage("AkismetUrl", "*")%>
    </li>
</ol>
