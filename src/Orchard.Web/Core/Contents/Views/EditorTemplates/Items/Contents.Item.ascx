<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<div class="sections">
    <div class="primary"><%
        Html.Zone("primary");
        Html.ZonesExcept("secondary"); %>
    </div>
    <div class="secondary">
        <% Html.Zone("secondary");%>
        <fieldset>
            <input class="button primaryAction" type="submit" name="submit.Save" value="<%=_Encoded("Save") %>"/>
        </fieldset>
    </div>
</div>