<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<AdminBlogsViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<h1><%=Html.TitleForPage(T("Manage Blogs").ToString()) %></h1>
<%-- todo: Add helper text here when ready. <p><%=_Encoded("Possible text about setting up and managing a blog goes here.") %></p> --%><%
if (Model.Entries.Count() > 0) { %>
<div class="actions"><a class="add button primaryAction" href="<%=Url.BlogCreate() %>"><%=_Encoded("New Blog") %></a></div>
<%=Html.UnorderedList(Model.Entries, (entry, i) => {
        // Add blog post count rendering into "meta" zone
        entry.ContentItemViewModel.Zones.AddAction("meta", html => {
            int draftCount = entry.TotalPostCount - entry.ContentItemViewModel.Item.PostCount;
            int totalPostCount = entry.TotalPostCount;
            var draftText = (draftCount == 0 ? "": string.Format(" ({0} draft{1})", draftCount, draftCount == 1 ? "" : "s"));
            
            var linkContent = _Encoded("{0} post{1}{2}", totalPostCount, totalPostCount == 1 ? "" : "s", draftText);
            
            html.ViewContext.Writer.Write(html.Link(linkContent.ToString(), Url.BlogForAdmin(entry.ContentItemViewModel.Item.Slug)));
        });

        // Display the summary for the blog post
        return Html.DisplayForItem(entry.ContentItemViewModel).ToHtmlString();
    }, "blogs contentItems")%>
<div class="actions"><a class="add button primaryAction" href="<%=Url.BlogCreate() %>"><%=_Encoded("New Blog") %></a></div><%
} else { %>
<div class="info message"><%=T("There are no blogs for you to see. Want to <a href=\"{0}\">add one</a>?", Url.BlogCreate()).ToString()%></div><%
} %>