<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ItemEditorModel<BlogPost>>" %>
<%@ Import Namespace="Orchard.ContentManagement.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<% Html.AddTitleParts(Model.Item.Title); %>
<div class="sections">
    <div class="primary">
        <%=Html.EditorZone("primary") %>
        <%=Html.EditorZonesExcept("secondary") %>
    </div>
    <div class="secondary">
        <%=Html.EditorZone("secondary")%>
        <fieldset>
            <input class="button" type="submit" name="submit.Save" value="Save"/>
        </fieldset>
    </div>
</div>
