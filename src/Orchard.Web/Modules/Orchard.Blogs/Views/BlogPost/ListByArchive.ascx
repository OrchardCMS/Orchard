<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BlogPostArchiveViewModel>" %>
<%@ Import Namespace="Orchard.Blogs.Extensions"%>
<%@ Import Namespace="Orchard.Blogs.ViewModels"%>
<h1><%=Html.TitleForPage(T("Archives").ToString(), Model.ArchiveData.Year.ToString(), Model.ArchiveData.Month > 0 ? new DateTime(Model.ArchiveData.Year, Model.ArchiveData.Month, 1).ToString("MMMM") : null, Model.ArchiveData.Day > 0 ? Model.ArchiveData.Day.ToString() : null)%></h1>
<%=T("Archives").ToString()
%> / <%=Html.Link(Model.ArchiveData.Year.ToString(), Url.BlogArchiveYear(Model.Blog.Slug, Model.ArchiveData.Year))
%><%=Model.ArchiveData.Month > 0 ? string.Format(" / {0}", Html.Link(Model.ArchiveData.ToDateTime().ToString("MMMM"), Url.BlogArchiveMonth(Model.Blog.Slug, Model.ArchiveData.Year, Model.ArchiveData.Month))) : ""
%><%=Model.ArchiveData.Day > 0 ? string.Format(" / {0}", Html.Link(Model.ArchiveData.Day.ToString(), Url.BlogArchiveDay(Model.Blog.Slug, Model.ArchiveData.Year, Model.ArchiveData.Month, Model.ArchiveData.Day))) : ""
%>
<%=Html.UnorderedList(Model.BlogPosts, (c, i) => Html.DisplayForItem(c).ToHtmlString(), "blogPosts contentItems")%>