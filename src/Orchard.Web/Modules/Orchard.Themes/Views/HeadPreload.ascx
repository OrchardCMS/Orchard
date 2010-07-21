<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%
// a CSS file for styling things (e.g. content item edit buttons) for users with elevated privileges (in this case, anyone who is authenticated)
if (Request.IsAuthenticated) { Html.RegisterStyle("special.css"); }
                                                                          
Html.RegisterScript("jquery-1.4.2.js", "1"); // <- change to .min.js for use on a real site :) 
Html.RegisterFootScript("base.js", "1");
%>