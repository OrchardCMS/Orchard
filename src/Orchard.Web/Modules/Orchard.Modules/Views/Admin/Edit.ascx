<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ModuleEditViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.Html"%>
<%@ Import Namespace="Orchard.Modules.ViewModels"%>
<h1><%=Html.TitleForPage(T("Edit Module: {0}", Model.Name).ToString()) %></h1>
<p><%=_Encoded("Edit the module. Maybe show module's features.") %></p>