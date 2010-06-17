<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<Orchard.Core.Contents.ViewModels.DisplayItemViewModel>" %>

<div class="preview">
    <%: Html.DisplayForItem(m=>m.Content) %>
</div>
