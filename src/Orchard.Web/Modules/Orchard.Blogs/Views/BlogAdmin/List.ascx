<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<AdminBlogsViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<h1><%: Html.TitleForPage(T("Manage Blogs").ToString()) %></h1>
<%-- todo: Add helper text here when ready. <p><%: T("Possible text about setting up and managing a blog goes here.") %></p> --%><%
if (Model.Entries.Count() > 0) { %>
<div class="actions"><a class="add button primaryAction" href="<%: Url.BlogCreate() %>"><%: T("New Blog") %></a></div>
<%: Html.UnorderedList(Model.Entries, (entry, i) => {
        // Add blog post count rendering into "meta" zone
        entry.ContentItemViewModel.Zones.AddAction("meta", html => {
            int draftCount = entry.TotalPostCount - entry.ContentItemViewModel.Item.PostCount;
            int totalPostCount = entry.TotalPostCount;
                        
            var linkText = T.Plural("1 post", "{0} posts", totalPostCount).ToString();
            if (draftCount > 0){
                linkText = linkText + " (" + T.Plural("1 draft", "{0} drafts", draftCount).ToString() + ")";
            }
            
            html.ViewContext.Writer.Write(html.Link(linkText, Url.BlogForAdmin(entry.ContentItemViewModel.Item.Slug)));
        });

        // Display the summary for the blog post
        return Html.DisplayForItem(entry.ContentItemViewModel);
    }, "blogs contentItems")%><%
} else { %>
<div class="info message"><%:T("There are no blogs for you to see. Want to <a href=\"{0}\">add one</a>?", Url.BlogCreate())%></div><%
} %>