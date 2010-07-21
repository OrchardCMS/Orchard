<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Core.Localization.ViewModels.EditLocalizationViewModel>" %>
<%
Html.RegisterStyle("admin.css");
Html.RegisterStyle("base.css");
if (Model.ContentLocalizations.MasterId > 0) { %>
<fieldset class="localization culture-selection"><%
    if (Model.MasterContentItem != null) { %>
    <fieldset class="culture-selected">
        <label for="SelectedCulture"><%:T("Content Localization") %></label>
        <div><%:T("This is the {0} variation of {1}.",
           Html.DropDownList("SelectedCulture", new SelectList(Model.SiteCultures, Model.SelectedCulture)),
           Html.ItemEditLink(Model.MasterContentItem)) %></div>
    </fieldset><%
    }
    if (Model.ContentLocalizations.Localizations.Count() > 0) { //todo: find a good place for this info on the content edit page %>
    <dl class="content-localization">
        <dt><%:Model.MasterContentItem != null ? T("Other translations:") : T("Translations:") %></dt>
        <dd class="content-localizations">
            <%:Html.UnorderedList(Model.ContentLocalizations.Localizations, (c, i) => Html.ItemEditLink(c.Culture.Culture, c), "localizations") %>
        </dd>
    </dl><%
    } %>
</fieldset><%
} %>