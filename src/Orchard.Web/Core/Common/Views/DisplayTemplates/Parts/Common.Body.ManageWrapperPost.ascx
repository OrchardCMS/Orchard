<%@ Control Language="C#" Inherits="Orchard.Mvc.ViewUserControl<BodyDisplayViewModel>" %>
<%@ Import Namespace="Orchard.Core.Common.ViewModels"%>
<%-- begin: knowingly broken HTML (hence the ManageWrapperPre and ManageWrapperPost templates)
we need "wrapper templates" (among other functionality) in the future of UI composition
please do not delete or the front end will be broken when the user is authenticated. --%>
</div>
<%-- begin: knowingly broken HTML --%>