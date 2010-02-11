<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<ContentItemViewModel<BlogPost>>" %>
<%@ Import Namespace="Orchard.Themes"%>
<%@ Import Namespace="Orchard.Extensions"%>
<%@ Import Namespace="Orchard.ContentManagement"%>
<%@ Import Namespace="Orchard.Core.Common.Models"%>
<%@ Import Namespace="Orchard.Mvc.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>

<ul>
    <li class="properties">
    <h3><%=Html.Link(Html.Encode(Model.Item.Title), Url.BlogPostEdit(Model.Item.Blog.Slug, Model.Item.Id))%></h3>

        <ul>
            <li>
            <%if (Model.IsPublished)
              { %>
            <img class="icon" src="<%=ResolveUrl("~/Packages/Orchard.Blogs/Content/Admin/images/online.gif") %>" alt="<%=_Encoded("Online") %>" title="<%=_Encoded("The page is currently online") %>" /><%=_Encoded(" Published")%>
             <% }
              else
              { %>
            <img class="icon" src="<%=ResolveUrl("~/Packages/Orchard.Blogs/Content/Admin/images/offline.gif") %>" alt="<%=_Encoded("Offline") %>" title="<%=_Encoded("The page is currently offline") %>" /><%=_Encoded(" Not Published")%>
            <% } %>
             &nbsp;&#124;&nbsp;
            </li>
   
             <li>
            <% if (Model.IsDraft) { %>
             <img class="icon" src="<%=ResolveUrl("~/Packages/Orchard.Blogs/Content/Admin/images/draft.gif") %>" alt="<%=_Encoded("Draft") %>" title="<%=_Encoded("The post has a draft") %>" /><%=Html.PublishedState(Model.Item)%>
              <% }
              else
              { %>
            <%=_Encoded("No draft")%>
             <% } %>
             &nbsp;&#124;&nbsp;
            </li>
 
             <%--This should show publised date, last modified, or scheduled.
             <li>
             <img class="icon" src="<%=ResolveUrl("~/Packages/Orchard.Blogs/Content/Admin/images/scheduled.gif") %>" alt="<%=_Encoded("Scheduled") %>" title="<%=_Encoded("The post is scheduled for publishing") %>" /><%=_Encoded("Scheduled")%>
             &nbsp;&#124;&nbsp;
             </li>--%>
 
             <li>
            <%=_Encoded("By {0}", Model.Item.Creator.UserName)%>
             </li>                   
        </ul>               
    </li>

    <li class="related">
    
            <%if (Model.IsPublished){ %>
        <a href="<%=Url.BlogPost(Model.Item.Blog.Slug, Model.Item.Slug) %>" title="<%=_Encoded("View Post")%>"><%=_Encoded("View")%></a><%=_Encoded(" | ")%>
        <% } %>

        <a href="<%=Url.BlogPostEdit(Model.Item.Blog.Slug, Model.Item.Id) %>" title="<%=_Encoded("Edit Post")%>"><%=_Encoded("Edit")%></a><%=_Encoded(" | ")%>

        <%if (Model.Item.ContentItem.VersionRecord.Published == false) { // todo: (heskew) be smart about this and maybe have other contextual actions - including view/preview for view up there ^^
                    using (Html.BeginFormAntiForgeryPost(Url.BlogPostPublish(Model.Item.Blog.Slug, Model.Item.Id), FormMethod.Post, new { @class = "inline" })) { %>

        <button type="submit" class="linkButton" title="<%=_Encoded("Publish") %>"><%=_Encoded("Publish")%></button><%=_Encoded(" | ")%><%
                    }
                } %>

        <% using (Html.BeginFormAntiForgeryPost(Url.BlogPostDelete(Model.Item.Blog.Slug, Model.Item.Id), FormMethod.Post, new { @class = "inline" })) { %>
        <button type="submit" class="linkButton" title="<%=_Encoded("Delete") %>"><%=_Encoded("Delete") %></button>
        <%
                } %>
                
        <br /><%Html.Zone("meta");%>
    </li>

    <li style="clear:both;"></li>
</ul>