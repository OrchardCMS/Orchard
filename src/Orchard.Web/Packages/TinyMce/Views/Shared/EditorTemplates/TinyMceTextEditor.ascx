<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<string>" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Import Namespace="Orchard.Mvc.Html" %>
<% Html.RegisterScript("tiny_mce.js"); %>
<%=Html.TextArea("", Model, 25, 80, new { @class = "html" }) %>
<script type="text/javascript">
    tinyMCE.init({
        theme: "advanced",
        mode: "specific_textareas",
        editor_selector: "html",
        plugins: "fullscreen,autoresize,searchreplace",
        theme_advanced_toolbar_location: "top",
        theme_advanced_toolbar_align: "left",
        theme_advanced_buttons1: "search,replace,|,cut,copy,paste,|,undo,redo,|,image,|,link,unlink,charmap,emoticon,codeblock,|,bold,italic,|,numlist,bullist,formatselect,|,code,fullscreen",
        theme_advanced_buttons2: "",
        theme_advanced_buttons3: ""
    });
</script>
