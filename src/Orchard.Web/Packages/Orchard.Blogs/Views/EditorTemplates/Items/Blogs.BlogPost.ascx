<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ItemEditorModel<BlogPost>>" %>
<%@ Import Namespace="Orchard.ContentManagement.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<% Html.AddTitleParts(Model.Item.Title); %>
<div class="sections">
    <div class="primary">
        <% Html.Zone("primary"); %>
        <% Html.ZonesExcept("secondary"); %>
    </div>
    <div class="secondary">
        <% Html.Zone("secondary");%>
        <fieldset>
            <input class="button" type="submit" name="submit.Save" value="Save"/>
        </fieldset>
    </div>
</div>
