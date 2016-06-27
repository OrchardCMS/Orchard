Feature: Pages
    In order to add content pages to my site
    As an author
    I want to create, publish and edit pages

Scenario: In the admin (menu) there is a link to create a Page
    Given I have installed Orchard

    When I go to "Admin"
    Then I should see "<a href="/Admin/Contents/Create/Page"[^>]*>Page</a>"

    # I can create and publish a new Page
    When I go to "Admin/Contents/Create/Page"
        And I fill in
            | name | value |
            | Title.Title | Super Duper |
            | LayoutPart.LayoutEditor.Data | { "type": "Content", "data": "TypeName=Orchard.Layouts.Elements.Text&Content=This+is+super", "isTemplated": false, "contentType": "Orchard.Layouts.Elements.Text", "contentTypeLabel": "Text", "contentTypeClass": "text", "html": "This is super", "hasEditor": true } |
        And I hit "Publish Now"
        And I go to "super-duper"
    Then I should see "<h1[^>]*>.*?Super Duper.*?</h1>"
        And I should see "This is super."

    # If I create a page which gets a conflicting path generated its path is made to be unique
    When I go to "Admin/Contents/Create/Page"
        And I fill in
            | name | value |
            | Title.Title | Super Duper |
            | LayoutPart.LayoutEditor.Data | { "type": "Content", "data": "TypeName=Orchard.Layouts.Elements.Text&Content=This+is+super+number+two", "isTemplated": false, "contentType": "Orchard.Layouts.Elements.Text", "contentTypeLabel": "Text", "contentTypeClass": "text", "html": "This is super number two", "hasEditor": true } |
        And I hit "Publish Now"
        And I go to "super-duper-2"
    Then I should see "<h1[^>]*>.*?Super Duper.*?</h1>"
        And I should see "This is super number two."

    # A new page marked to be the home page and publish does take over the home page and is also accessible from its own standard path
    When I go to "Admin/Contents/Create/Page"
        And I fill in
            | name | value |
            | Title.Title | Another |
            | AutoroutePart.PromoteToHomePage | true |
        And I hit "Publish Now"
        And I go to "/"
    Then I should see "<h1>Another</h1>"
    When I go to "another"
    Then the status should be 200 "OK"

    # A new page marked to be the home page but only saved as draft does not take over the home page
    When I go to "Admin/Contents/Create/Page"
        And I fill in
            | name | value |
            | Title.Title | Drafty |
            | AutoroutePart.PromoteToHomePage | true |
        And I hit "Save"
        And I go to "/"
    Then I should see "<h1>Another</h1>"
