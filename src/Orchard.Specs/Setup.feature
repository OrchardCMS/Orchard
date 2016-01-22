Feature: Setup
    In order to install orchard
    As a new user
    I want to setup a new site from the default screen

Scenario: Root request shows setup form
    Given I have a clean site with
            | extension | names |
            | Module | Orchard.Setup, Orchard.Layouts, Orchard.Pages, Orchard.ContentPicker, Orchard.Blogs, Orchard.MediaLibrary, Orchard.Modules, Orchard.Packaging, Orchard.PublishLater, Orchard.Themes, Orchard.Scripting, Orchard.Widgets, Orchard.Users, Orchard.ContentTypes, Orchard.Roles, Orchard.Comments, Orchard.jQuery, Orchard.Tags, TinyMce, Orchard.Recipes, Orchard.Warmup, Orchard.Alias, Orchard.Forms, Orchard.Tokens, Orchard.Autoroute, Orchard.Projections, Orchard.Fields, Orchard.MediaProcessing, Orchard.OutputCache, Orchard.Taxonomies, Orchard.Workflows, Orchard.Scripting.CSharp |
            | Core | Common, Containers, Dashboard, Feeds, Navigation, Contents, Scheduling, Settings, Shapes, XmlRpc, Title, Reports |
            | Theme | SafeMode |
    When I go to "/"
    Then I should see "Welcome to Orchard"
        And I should see "Finish Setup"
        And the status should be 200 "OK"

Scenario: Setup folder also shows setup form
    Given I have a clean site with
            | extension | names |
            | Module | Orchard.Setup, Orchard.Layouts, Orchard.Pages, Orchard.ContentPicker, Orchard.Blogs, Orchard.MediaLibrary, Orchard.Modules, Orchard.Packaging, Orchard.PublishLater, Orchard.Themes, Orchard.Scripting, Orchard.Widgets, Orchard.Users, Orchard.ContentTypes, Orchard.Roles, Orchard.Comments, Orchard.jQuery, Orchard.Tags, TinyMce, Orchard.Recipes, Orchard.Warmup, Orchard.Alias, Orchard.Forms, Orchard.Tokens, Orchard.Autoroute, Orchard.Projections, Orchard.Fields, Orchard.MediaProcessing, Orchard.OutputCache, Orchard.Taxonomies, Orchard.Workflows, Orchard.Scripting.CSharp |
            | Core | Common, Containers, Dashboard, Feeds, Navigation, Contents, Scheduling, Settings, Shapes, XmlRpc, Title, Reports |
            | Theme | SafeMode |
    When I go to "/Setup"
    Then I should see "Welcome to Orchard"
        And I should see "Finish Setup"
        And the status should be 200 "OK"

Scenario: Some of the initial form values are required
    Given I have a clean site with
            | extension | names |
            | Module | Orchard.Setup, Orchard.Layouts, Orchard.Pages, Orchard.ContentPicker, Orchard.Blogs, Orchard.MediaLibrary, Orchard.Modules, Orchard.Packaging, Orchard.PublishLater, Orchard.Themes, Orchard.Scripting, Orchard.Widgets, Orchard.Users, Orchard.ContentTypes, Orchard.Roles, Orchard.Comments, Orchard.jQuery, Orchard.Tags, TinyMce, Orchard.Recipes, Orchard.Warmup, Orchard.Alias, Orchard.Forms, Orchard.Tokens, Orchard.Autoroute, Orchard.Projections, Orchard.Fields, Orchard.MediaProcessing, Orchard.OutputCache, Orchard.Taxonomies, Orchard.Workflows, Orchard.Scripting.CSharp |
            | Core | Common, Containers, Dashboard, Feeds, Navigation, Contents, Scheduling, Settings, Shapes, XmlRpc, Title, Reports |
            | Theme | SafeMode |
    When I go to "/Setup"
        And I hit "Finish Setup"
    Then I should see "Site name is required."
        And I should see "Password is required."
        And I should see "Password confirmation is required."

Scenario: Calling setup on a brand new install
    Given I have a clean site with
            | extension | names |
            | Module | Orchard.Setup, Orchard.Layouts, Orchard.Pages, Orchard.ContentPicker, Orchard.Blogs, Orchard.MediaLibrary, Orchard.Modules, Orchard.Packaging, Orchard.PublishLater, Orchard.Themes, Orchard.Scripting, Orchard.Widgets, Orchard.Users, Orchard.ContentTypes, Orchard.Roles, Orchard.Comments, Orchard.jQuery, Orchard.Tags, TinyMce, Orchard.Recipes, Orchard.Warmup, Orchard.Alias, Orchard.Forms, Orchard.Tokens, Orchard.Autoroute, Orchard.Projections, Orchard.Fields, Orchard.MediaProcessing, Orchard.OutputCache, Orchard.Taxonomies, Orchard.Workflows, Orchard.Scripting.CSharp |
            | Core | Common, Containers, Dashboard, Feeds, Navigation, Contents, Scheduling, Settings, Shapes, XmlRpc, Title, Reports |
            | Theme | SafeMode, TheAdmin, TheThemeMachine |
        And I am on "/Setup"
    When I fill in 
            | name | value |
            | SiteName | My Site |
            | AdminPassword | 6655321 |
            | ConfirmPassword | 6655321 |
        And I hit "Finish Setup"
        And I go to "/"
    Then I should see "My Site"
        And I should see "Welcome, <strong><a href="/Users/Account/ChangePassword">admin</a></strong>!"
