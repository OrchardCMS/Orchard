<%@ Control Language="C#" AutoEventWireup="true" Inherits="Orchard.Mvc.ViewUserControl<BlogArchivesViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.Models"%>
<% Html.RegisterStyle("archives.css"); %>
<% Html.RegisterFootScript("archives.js"); %>
<div class="archives">
    <h3><%: T("Archives") %></h3><%
    if (Model.Archives.Count() > 0) {
        if (Model.Archives.Count() > 20) { %>
    <ul class="years"><%
            int lastYear = Model.Archives.First().Key.Year;
            int firstYear = Model.Archives.Last().Key.Year;

            for (int year = lastYear; year >= firstYear; year--) {
                var yearMonths = Model.Archives.Where(m => m.Key.Year == year);

                if (year == lastYear) { %>
        <li>
            <h4><%=year %></h4><%
                }
                else { %>
        <li class="previous">
            <h4><%=year %> <span>(<%=yearMonths.Sum(ym => ym.Value) %>)</span></h4><%
                } %>
            <%=Html.UnorderedList(yearMonths, (t, i) => Html.Link(string.Format("{0:MMMM} ({1})", t.Key.ToDateTime(), t.Value), Url.BlogArchiveMonth(Model.Blog.Slug, t.Key.Year, t.Key.Month)), "archiveMonthList") %>
        </li><% 
            } %>
    </ul><%
        }
        else { %>
        <%=Html.UnorderedList(Model.Archives, (t, i) => Html.Link(string.Format("{0:MMMM yyyy} ({1})", t.Key.ToDateTime(), t.Value), Url.BlogArchiveMonth(Model.Blog.Slug, t.Key.Year, t.Key.Month)), "archiveMonthList") %><%
        }
    }
    else { %>
    <div class="message info"><%: T("None found")%></div><%
    } %>
</div>