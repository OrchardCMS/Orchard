<%@ Page Language="C#" Inherits="Orchard.Mvc.ViewPage<BaseViewModel>"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<h1><%: Html.TitleForPage(T("Data migration").ToString()) %></h1>
<p><%: Html.ActionLink(T("Update database").ToString(), "UpdateDatabase", "DataMigration") %></p>

