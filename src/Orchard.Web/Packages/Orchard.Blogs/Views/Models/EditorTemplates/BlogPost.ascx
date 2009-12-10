<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ItemEditorModel<BlogPost>>" %>
<%@ Import Namespace="Orchard.Models.ViewModels"%>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<div class="sections">
    <div class="primary">
        <%=Html.EditorZone("body") %>
        <%=Html.EditorZonesExcept("sidebar") %>
    </div>
    <div class="secondary">
        <%=Html.EditorZone("sidebar") %>
        <fieldset>
            <input class="button" type="submit" name="submit.Save" value="Save"/>
        </fieldset>
    </div>
</div>
