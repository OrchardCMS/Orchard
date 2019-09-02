Feature: Comments
    In order to enable simple comment capabilities on my site
    As an author
    I want to allow comments to be safely posted on specific content item pages

Scenario: HTML markup in any given comment is encoded
    Given I have installed Orchard
    When I go to "admin/blogs/create"
        And I fill in
            | name | value |
            | Title.Title | My Blog |
        And I hit "Publish"
        And I go to "admin/blogs"
        And I follow "My Blog"
        And I follow "New Post" where class name has "primaryAction"
        And I fill in
            | name | value |
            | Title.Title | My Post |
            | Body.Text | Hi there. |
        And I hit "Publish"
        And I go to "my-blog/my-post"
        And I fill in
            | name | value |
            | Comments.CommentText | This is<br id="bad-br" />a <a href="#">link</a>. |
        And I hit "Submit Comment"
        And I am redirected
        # because the ToUrlString extension method breaks in this specific (test) environment, the returnUrl is broken...
        And I go to "my-blog/my-post"
    Then I should see "This is&lt;br id=&quot;bad-br&quot; /&gt;a &lt;a href"
        And I should not see "<br id="bad-br" />"
    # another workaround because of ToUrlString in this environment
    When I go to "Users/Account/LogOff"
        And I am redirected
        And I go to "my-blog/my-post"
        And I fill in
            | name | value |
            | Comments.Author | Some One |
            | Comments.CommentText | This is<br id="bad-anon-br" />a <a href="#">link</a>. |
        And I hit "Submit Comment"
        And I am redirected
        # because the ToUrlString extension method breaks in this specific (test) environment, the returnUrl is broken...
        And I go to "my-blog/my-post"
        # And I go to "my-blog/my-post"
    Then I should see "This is&lt;br id=&quot;bad-anon-br&quot; /&gt;a &lt;a href"
        And I should not see "<br id="bad-anon-br" />"

    # Moderated comments are not displayed
    When I go to "users/account/logon"
        And I fill in
            | name | value |
            | userNameOrEmail | admin |
            | password | 6655321 |
        And I hit "Sign In"
        And I am redirected
        And I go to "admin/settings/comments"
        And I fill in
            | name | value |
            | CommentSettings.ModerateComments | true |
        And I hit "Save"
        And I am redirected
    Then I should see "Settings updated"
    When I go to "users/account/logoff"
        And I go to "my-blog/my-post"
        And I fill in
            | name | value |
            | Comments.Author | Bill |
            | Comments.CommentText | This is a moderated comment. |
        And I hit "Submit Comment"
        And I am redirected
        # because the ToUrlString extension method breaks in this specific (test) environment, the returnUrl is broken...
        And I go to "my-blog/my-post"
    Then I should see "Hi there"
        And I should not see "This is a moderated comment"
