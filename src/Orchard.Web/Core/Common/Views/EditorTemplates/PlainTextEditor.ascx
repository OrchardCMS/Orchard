<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BodyEditorViewModel>" %>
<%@ Import Namespace="Orchard.Core.Common.ViewModels"%>
<%: Html.TextArea("Text", Model.Text, 25, 80, new { @class = Model.Format }) %>