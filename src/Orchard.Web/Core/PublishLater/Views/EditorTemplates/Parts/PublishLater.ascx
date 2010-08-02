<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Core.PublishLater.ViewModels.PublishLaterViewModel>" %>
<% Html.RegisterStyle("datetime.css"); %>
<% Html.RegisterStyle("jquery-ui-1.7.2.custom.css"); %>
<% Html.RegisterStyle("ui.datepicker.css"); %>
<% Html.RegisterStyle("ui.timepickr.css"); %>
<% Html.RegisterFootScript("jquery.ui.core.js"); %>
<% Html.RegisterFootScript("jquery.ui.widget.js"); %>
<% Html.RegisterFootScript("jquery.ui.datepicker.js"); %>
<% Html.RegisterFootScript("jquery.utils.js"); %>
<% Html.RegisterFootScript("ui.timepickr.js"); %>
<fieldset>
    <legend><%: T("Publish Settings")%></legend>
    <div>
        <%: Html.RadioButton("Command", "SaveDraft", Model.ContentItem.VersionRecord == null || !Model.ContentItem.VersionRecord.Published, new { id = ViewData.TemplateInfo.GetFullHtmlFieldId("Command_SaveDraft") })%>
        <label class="forcheckbox" for="<%:ViewData.TemplateInfo.GetFullHtmlFieldId("Command_SaveDraft") %>"><%: T("Save Draft")%></label>
    </div>
    <div>
        <%: Html.RadioButton("Command", "PublishNow", Model.ContentItem.VersionRecord != null && Model.ContentItem.VersionRecord.Published, new { id = ViewData.TemplateInfo.GetFullHtmlFieldId("Command_PublishNow") })%>
        <label class="forcheckbox" for="<%:ViewData.TemplateInfo.GetFullHtmlFieldId("Command_PublishNow") %>"><%: T("Publish Now")%></label>
    </div>
    <div>
        <%: Html.RadioButton("Command", "PublishLater", Model.ScheduledPublishUtc != null, new { id = ViewData.TemplateInfo.GetFullHtmlFieldId("Command_PublishLater") }) %>
        <label class="forcheckbox" for="<%:ViewData.TemplateInfo.GetFullHtmlFieldId("Command_PublishLater") %>"><%: T("Publish Later")%></label>
    </div>
    <div>
        <label class="forpicker" for="<%:ViewData.TemplateInfo.GetFullHtmlFieldId("ScheduledPublishUtcDate") %>"><%: T("Date")%></label>
        <%: Html.EditorFor(m => m.ScheduledPublishUtcDate)%>
        <label class="forpicker" for="<%:ViewData.TemplateInfo.GetFullHtmlFieldId("ScheduledPublishUtcTime") %>"><%: T("Time")%></label>
        <%: Html.EditorFor(m => m.ScheduledPublishUtcTime)%>
    </div>
</fieldset>
<script type="text/javascript">    $(function () {
        //todo: (heskew) make a plugin
        $("label.forpicker").each(function () {
            var $this = $(this);
            var pickerInput = $("#" + $this.attr("for"));
            pickerInput.data("hint", $this.text());
            if (!pickerInput.val()) {
                pickerInput.addClass("hinted")
            .val(pickerInput.data("hint"))
            .focus(function () { var $this = $(this); if ($this.val() == $this.data("hint")) { $this.removeClass("hinted").val("") } })
            .blur(function () { var $this = $(this); setTimeout(function () { if (!$this.val()) { $this.addClass("hinted").val($this.data("hint")) } }, 300) });
            }
        });
        $(<%=string.Format("\"#{0}\"", ViewData.TemplateInfo.GetFullHtmlFieldId("ScheduledPublishUtcDate")) %>).datepicker({ showAnim: "" }).focus(function () { $(<%=string.Format("\"#{0}\"", ViewData.TemplateInfo.GetFullHtmlFieldId("Command_PublishLater")) %>).attr("checked", "checked") });
        $(<%=string.Format("\"#{0}\"", ViewData.TemplateInfo.GetFullHtmlFieldId("ScheduledPublishUtcTime")) %>).timepickr().focus(function () { $(<%=string.Format("\"#{0}\"", ViewData.TemplateInfo.GetFullHtmlFieldId("Command_PublishLater")) %>).attr("checked", "checked") });
    })</script>