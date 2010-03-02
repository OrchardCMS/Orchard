<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<AdminViewModel>" %>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<h1><%=Html.TitleForPage(T("Welcome to the Orchard administration dashboard").ToString())%></h1>

<p>
<%=_Encoded("This is the place where you can manage your web site, its appearance and its contents. Please take a moment to explore the different menu items on the left of the screen to familiarize yourself with the features of the application. For example, try to change the theme through the “Manage Themes” menu entry. You can also create new pages and manage existing ones through the “Manage Pages” menu entry or create blogs through “Manage Blogs”.") %></p>

<p>
<%=_Encoded("Have fun!") %><br />
<%=_Encoded("The Orchard Team") %>
</p>

