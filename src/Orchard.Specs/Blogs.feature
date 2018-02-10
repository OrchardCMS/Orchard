Feature: Blog
    In order to add blogs to my site
    As an author
    I want to create blogs and create, publish and edit blog posts

Scenario: In the admin (menu) there is a link to create a Blog
    Given I have installed Orchard
    When I go to "admin"
    Then I should see "<a[^>]*href="/Admin/Blogs/Create"[^>]*>Blog</a>"

Scenario: I can create a new blog and blog post
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
        And I am redirected
    Then I should see "The Blog Post has been created and published."
    When I go to "my-blog"
    Then I should see "<h1[^>]*>.*?My Blog.*?</h1>"
        And I should see "<h1[^>]*>.*?My Post.*?</h1>"
    When I go to "my-blog/my-post"
    Then I should see "<h1[^>]*>.*?My Post.*?</h1>"
        And I should see "Hi there."

Scenario: I can create a new blog with multiple blog posts each with the same title and unique slugs are generated or given for said posts
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
    Then I should see "<h1[^>]*>.*?My Post.*?</h1>"
        And I should see "Hi there."
    When I go to "admin/blogs"
        And I follow "My Blog"
        And I follow "New Post" where class name has "primaryAction"
        And I fill in
            | name | value |
            | Title.Title | My Post |
            | Body.Text | Hi there, again. |
        And I hit "Publish"
        And I go to "my-blog/my-post-2"
    Then I should see "<h1[^>]*>.*?My Post.*?</h1>"
        And I should see "Hi there, again."
    When I go to "admin/blogs"
        And I follow "My Blog"
        And I follow "New Post" where class name has "primaryAction"
        And I fill in
            | name | value |
            | Title.Title | My Post |
            | AutoroutePart.CurrentUrl | my-blog/my-post |
            | Body.Text | Are you still there? |
        And I hit "Publish"
        And I go to "my-blog/my-post-3"
    Then I should see "<h1[^>]*>.*?My Post.*?</h1>"
        And I should see "Are you still there?"

Scenario: When viewing a blog the user agent is given an RSS feed of the blog's posts
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
        And I am redirected
        And I go to "my-blog/my-post"
    Then I should see "<link rel="alternate" type="application/rss\+xml" title="My Blog" href="/rss\?containerid=\d+" />"

Scenario: Enabling remote blog publishing inserts the appropriate metaweblogapi markup into the blog's page
    Given I have installed Orchard
        And I have enabled "XmlRpc"
        And I have enabled "Orchard.Blogs.RemotePublishing"
    When I go to "admin/blogs/create"
        And I fill in
            | name | value |
            | Title.Title | My Blog |
        And I hit "Publish"
        And I go to "my-blog"
    Then I should see "<link href="[^"]*/XmlRpc/LiveWriter/Manifest" rel="wlwmanifest" type="application/wlwmanifest\+xml" />"
    When I go to "/XmlRpc/LiveWriter/Manifest"
    Then the content type should be "\btext/xml\b"
        And I should see "<manifest xmlns="http\://schemas\.microsoft\.com/wlw/manifest/weblog">"
        And I should see "<clientType>Metaweblog</clientType>"

Scenario: The virtual path of my installation when not at the root is reflected in the URL example for the slug field when creating a blog or blog post
    Given I have installed Orchard at "/OrchardLocal"
    When I go to "admin/blogs/create"
    Then I should see "<span>http\://localhost/OrchardLocal/</span>"
    When I fill in
        | name | value |
        | Title.Title | My Blog |
        And I hit "Publish"
        And I go to "admin/blogs"
        And I follow "My Blog"
        And I follow "New Post" where class name has "primaryAction"
    Then I should see "<span>http\://localhost/OrchardLocal/</span>"

Scenario: The virtual path of my installation when at the root is reflected in the URL example for the slug field when creating a blog or blog post
    Given I have installed Orchard at "/"
    When I go to "admin/blogs/create"
    Then I should see "<span>http\://localhost/</span>"
    When I fill in
        | name | value |
        | Title.Title | My Blog |
        And I hit "Publish"
        And I go to "admin/blogs"
        And I follow "My Blog"
        And I follow "New Post" where class name has "primaryAction"
    Then I should see "<span>http\://localhost/</span>"

Scenario: I set my blog to be the content for the home page and the posts for the blog be rooted to the app
    Given I have installed Orchard
    When I go to "admin/blogs/create"
        And I fill in
            | name | value |
            | Title.Title | My Blog |
            | AutoroutePart.PromoteToHomePage | true |
        And I hit "Publish"
        And I go to "admin/blogs"
        And I follow "My Blog"
        And I follow "New Post" where class name has "primaryAction"
        And I fill in
            | name | value |
            | Title.Title | My Post |
            | Body.Text | Hi there. |
        And I hit "Publish"
        And I am redirected
        And I go to "/"
    Then I should see "<h1>My Blog</h1>"
    When I go to "/my-blog"
    Then the status should be 200 "OK"
    When I go to "/"
    Then the status should be 200 "OK"
    When I go to "/my-post"
    Then I should see "<h1>My Post</h1>"

Scenario: I can create browse blog posts on several pages
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
            | Title.Title | My Post 1 |
        And I hit "Publish"
        And I am redirected
        And I go to "admin/blogs"
        And I follow "My Blog"
        And I follow "New Post" where class name has "primaryAction"
        And I fill in
            | name | value |
            | Title.Title | My Post 2 |
        And I hit "Publish"
        And I am redirected
        And I go to "admin/blogs"
        And I follow "My Blog"
        And I follow "New Post" where class name has "primaryAction"
        And I fill in
            | name | value |
            | Title.Title | My Post 3 |
        And I hit "Publish"
        And I am redirected
        And I go to "admin/blogs"
        And I follow "My Blog"
        And I follow "New Post" where class name has "primaryAction"
        And I fill in
            | name | value |
            | Title.Title | My Post 4 |
        And I hit "Publish"
        And I am redirected
        And I go to "admin/blogs"
        And I follow "My Blog"
        And I follow "New Post" where class name has "primaryAction"
        And I fill in
            | name | value |
            | Title.Title | My Post 5 |
        And I hit "Publish"
        And I am redirected
        And I go to "admin/blogs"
        And I follow "My Blog"
        And I follow "New Post" where class name has "primaryAction"
        And I fill in
            | name | value |
            | Title.Title | My Post 6 |
        And I hit "Publish"
        And I am redirected
        And I go to "admin/blogs"
        And I follow "My Blog"
        And I follow "New Post" where class name has "primaryAction"
        And I fill in
            | name | value |
            | Title.Title | My Post 7 |
        And I hit "Publish"
        And I am redirected
        And I go to "admin/blogs"
        And I follow "My Blog"
        And I follow "New Post" where class name has "primaryAction"
        And I fill in
            | name | value |
            | Title.Title | My Post 8 |
        And I hit "Publish"
        And I am redirected
        And I go to "admin/blogs"
        And I follow "My Blog"
        And I follow "New Post" where class name has "primaryAction"
        And I fill in
            | name | value |
            | Title.Title | My Post 9 |
        And I hit "Publish"
        And I am redirected
        And I go to "admin/blogs"
        And I follow "My Blog"
        And I follow "New Post" where class name has "primaryAction"
        And I fill in
            | name | value |
            | Title.Title | My Post 10 |
        And I hit "Publish"
        And I am redirected
        And I go to "admin/blogs"
        And I follow "My Blog"
        And I follow "New Post" where class name has "primaryAction"
        And I fill in
            | name | value |
            | Title.Title | My Post 11 |
        And I hit "Publish"
        And I am redirected
        And I go to "admin/blogs"
        And I follow "My Blog"
        And I follow "New Post" where class name has "primaryAction"
        And I fill in
            | name | value |
            | Title.Title | My Post 12 |
        And I hit "Publish"
        And I am redirected
    Then I should see "Your Blog Post has been created."
    When I go to "my-blog"
    Then I should see "<h1[^>]*>.*?My Blog.*?</h1>"
        And I should see "<h1[^>]*>.*?My Post 12.*?</h1>"
        And I should see "<h1[^>]*>.*?My Post 11.*?</h1>"
        And I should not see "<h1[^>]*>.*?My Post 10.*?</h1>"
    When I go to "my-blog?page=2"
    Then I should see "<h1[^>]*>.*?My Blog.*?</h1>"
        And I should see "<h1[^>]*>.*?My Post 1.*?</h1>"
        And I should see "<h1[^>]*>.*?My Post 2.*?</h1>"
        And I should not see "<h1[^>]*>.*?My Post 3.*?</h1>"

Scenario: I can create a new blog with a percent sign in the title and it gets stripped out of the slug
       Given I have installed Orchard
       When I go to "admin/blogs/create"
       And I fill in
         | name        | value   |
         | Title.Title | My Blog |
       And I hit "Publish"
       And I go to "admin/blogs"
       And I follow "My Blog"
       And I follow "New Post" where class name has "primaryAction"
       And I fill in
         | name        | value                 |
         | Title.Title | My Post with a % Sign |
         | Body.Text   | Hi there.             |
       And I hit "Publish"
       And I go to "my-blog/my-post-with-a-sign"
       Then I should see "<h1[^>]*>.*?My Post with a % Sign.*?</h1>"
       And I should see "Hi there."