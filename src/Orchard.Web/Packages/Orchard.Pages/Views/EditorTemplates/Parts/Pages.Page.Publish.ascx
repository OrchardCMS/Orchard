<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<Orchard.Pages.Models.Page>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<% Html.RegisterStyle("datetime.css"); %>
<% Html.RegisterStyle("jquery-ui-1.7.2.custom.css"); %>
<% Html.RegisterStyle("ui.datepicker.css"); %>
<% Html.RegisterStyle("ui.timepickr.css"); %>
<% Html.RegisterFootScript("datetime.js"); %>
<% Html.RegisterFootScript("jquery.ui.core.js"); %>
<% Html.RegisterFootScript("jquery.ui.widget.js"); %>
<% Html.RegisterFootScript("jquery.ui.datepicker.js"); %>
<% Html.RegisterFootScript("jquery.utils.js"); %>
<% Html.RegisterFootScript("ui.timepickr.js"); %>
<fieldset>
    <legend><%=_Encoded("Publish Settings")%></legend>
    <div>
        <%=Html.RadioButton("Command", "SaveDraft", Model.ContentItem.VersionRecord == null || !Model.ContentItem.VersionRecord.Published, new { id = "Command_SaveDraft" })%>
        <label class="forcheckbox" for="Command_SaveDraft"><%=_Encoded("Save Draft")%></label>
    </div>
    <div>
        <%=Html.RadioButton("Command", "PublishNow", Model.ContentItem.VersionRecord != null && Model.ContentItem.VersionRecord.Published, new { id = "Command_PublishNow" })%>
        <label class="forcheckbox" for="Command_PublishNow"><%=_Encoded("Publish Now")%></label>
    </div>
    <div>
        <%=Html.RadioButton("Command", "PublishLater", Model.ScheduledPublishUtc != null, new { id = "Command_PublishLater" }) %>
        <label class="forcheckbox" for="Command_PublishLater"><%=_Encoded("Publish Later")%></label>
        <label class="forpicker" for="ScheduledPublishUtcDate"><%=_Encoded("Date")%></label>
        <%=Html.EditorFor(m => m.ScheduledPublishUtcDate)%>
        <label class="forpicker" for="ScheduledPublishUtcTime"><%=_Encoded("Time")%></label>
        <%=Html.EditorFor(m => m.ScheduledPublishUtcTime)%>
    </div>
</fieldset>
<script type="text/javascript">$(function() {
    //todo: (heskew) make a plugin
    $("label.forpicker").each(function() {
        var $this = $(this);
        var pickerInput = $("#" + $this.attr("for"));
        pickerInput.data("hint", $this.text());
        if (!pickerInput.val()) {
            pickerInput.addClass("hinted")
            .val(pickerInput.data("hint"))
            .focus(function() { var $this = $(this); if ($this.val() == $this.data("hint")) { $this.removeClass("hinted").val("") } })
            .blur(function() { var $this = $(this); setTimeout(function() { if (!$this.val()) { $this.addClass("hinted").val($this.data("hint")) } }, 300) });
        }
    });
    $("input#ScheduledPublishUtcDate").datepicker().focus(function() { $("#Command_PublishLater").attr("checked", "checked") });
    $("input#ScheduledPublishUtcTime").timepickr().focus(function() { $("#Command_PublishLater").attr("checked", "checked") });
})</script>