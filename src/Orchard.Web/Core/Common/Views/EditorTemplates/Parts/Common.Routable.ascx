<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<RoutableEditorViewModel>" %>
<%@ Import Namespace="Orchard.Core.Common.ViewModels"%>
<% Html.RegisterFootScript("jquery.slugify.js"); %>
<fieldset>
    <%=Html.LabelFor(m => m.Title) %>
    <%=Html.TextBoxFor(m => m.Title, new { @class = "large text" }) %>
</fieldset>
<fieldset class="permalink">
    <label class="sub" for="Slug"><%=_Encoded("Permalink")%><br /><span>[todo: (heskew) need path to here]/</span></label>
    <span><%=Html.TextBoxFor(m => m.Slug, new { @class = "text" })%></span>
</fieldset>
<% using (this.Capture("end-of-page-scripts")) { %>
<script type="text/javascript">$(function(){$("input#Routable_Title").blur(function(){$(this).slugify({target:$("input#Routable_Slug"),url:"<%=Url.Action("Slugify", "Routable", new {area = "Common"}) %>"})})})</script>
<% } %>