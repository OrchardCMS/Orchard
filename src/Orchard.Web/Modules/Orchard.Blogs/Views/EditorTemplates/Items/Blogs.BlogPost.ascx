<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<BlogPost>>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<% Html.AddTitleParts(Model.Item.Title); %>
<div class="sections">
    <div class="primary"><%
        Html.Zone("primary");
        Html.ZonesExcept("secondary"); %>
    </div>
    <div class="secondary">
        <% Html.Zone("secondary");%>
        <fieldset>
            <input class="button primaryAction" type="submit" name="submit.Save" value="<%=_Encoded("Save") %>"/>
            <% if (Model.IsDraft) { %>
            <%=Html.ActionLink(T("Discard Draft").ToString(), "DiscardDraft", new { Area = "Orchard.Blogs", Controller = "BlogPostAdmin", id=Model.Item.Id }, new { @class = "button" })%>
            <% } %>
        </fieldset>
    </div>
</div>