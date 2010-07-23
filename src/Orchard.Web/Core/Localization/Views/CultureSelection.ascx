<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Core.Localization.ViewModels.AddLocalizationViewModel>" %>
<fieldset class="localization culture-selection">
    <label for="SelectedCulture"><%:T("Culture ") %></label>
    <%:Html.DropDownList("SelectedCulture", new SelectList(Model.SiteCultures, Model.SelectedCulture)) %>
</fieldset>