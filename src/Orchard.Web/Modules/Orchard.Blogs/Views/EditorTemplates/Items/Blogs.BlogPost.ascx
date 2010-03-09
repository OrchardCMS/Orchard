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
            <input class="button primaryAction" type="submit" name="submit.Save" value="<%=_Encoded("Save") %>"/><%
            //TODO: (erikpo) In the future, remove the HasPublished check so the user can delete the content item from here if the choose to
            if (Model.Item.HasDraft && Model.Item.HasPublished) { %>
            <%=Html.AntiForgeryTokenValueOrchardLink(T("Discard Draft").ToString(), Url.Action("DiscardDraft", new {Area = "Orchard.Blogs", Controller = "BlogPostAdmin", id = Model.Item.Id}), new {@class = "button"})%><%
            } %>
        </fieldset>
    </div>
</div>