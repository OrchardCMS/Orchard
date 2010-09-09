<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl" %>

<h1><%: Html.TitleForPage(T("Welcome to Orchard").ToString())%></h1>
<p><%: T("This is the place where you can manage your web site, its appearance and its contents. Please take a moment to explore the different menu items on the left of the screen to familiarize yourself with the features of the application. For example, try to change the theme through the “Manage Themes” menu entry. You can also create new pages and manage existing ones through the “Manage Pages” menu entry or create blogs through “Manage Blogs”.") %></p>
<p><%: T("Have fun!") %><br /><%: T("The Orchard Team") %></p>

<div style="border:1px solid black;" role="experimentation"><%:Display(Model) %></div>
