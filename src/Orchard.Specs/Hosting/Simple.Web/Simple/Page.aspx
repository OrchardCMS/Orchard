<%@ Page %>

<p>
    Hello again</p>
<p>
    RawUrl:
    <%=Page.Request.RawUrl%></p>
<p>
    Moving along to <a href="/hello-world">next page</a></p>
<p>
    <form action="<%=ResolveUrl("~/Simple/Results.aspx") %>" method="post">
    <input type="text" name="passthrough1" value="alpha" />
    <input type="hidden" name="passthrough2" value="beta" />
    <input type="hidden" name="input1" />
    <input type="submit" value="Go!" />
    </form>
</p>
