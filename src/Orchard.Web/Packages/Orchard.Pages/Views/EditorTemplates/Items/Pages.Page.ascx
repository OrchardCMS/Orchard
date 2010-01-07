<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ContentItemViewModel<Orchard.Pages.Models.Page>>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Pages.Models"%>
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
