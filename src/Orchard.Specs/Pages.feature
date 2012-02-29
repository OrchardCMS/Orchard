Feature: Pages
    In order to add content pages to my site
    As an author
    I want to create, publish and edit pages

Scenario: In the admin (menu) there is a link to create a Page
    Given I have installed Orchard
		And I have installed "Orchard.jQuery"
    When I go to "admin"
    Then I should see "<a href="/Admin/Contents/Create/Page"[^>]*>Page</a>"

Scenario: I can create and publish a new Page
    Given I have installed Orchard
    When I go to "admin/contents/create/page"
        And I fill in
            | name | value |
            | Title.Title | Super Duper |
            | Body.Text | This is super. |
        And I hit "Publish Now"
        And I go to "super-duper"
    Then I should see "<h1[^>]*>.*?Super Duper.*?</h1>"
        And I should see "This is super."

Scenario: If I create a page which gets a conflicting path generated its path is made to be unique
    Given I have installed Orchard
    When I go to "admin/contents/create/page"
        And I fill in
            | name | value |
            | Title.Title | Super Duper |
            | Body.Text | This is super. |
        And I hit "Publish Now"
        And I go to "super-duper"
    Then I should see "<h1[^>]*>.*?Super Duper.*?</h1>"
        And I should see "This is super."
    When I go to "admin/contents/create/page"
        And I fill in
            | name | value |
            | Title.Title | Super Duper |
            | Body.Text | This is super number two. |
        And I hit "Publish Now"
        And I go to "super-duper-2"
    Then I should see "<h1[^>]*>.*?Super Duper.*?</h1>"
        And I should see "This is super number two."

Scenario: A new page marked to be the home page and publish does take over the home page and is not accessible from its own standard path
    Given I have installed Orchard
    When I go to "admin/contents/create/page"
        And I fill in
            | name | value |
            | Title.Title | Super Duper |
            | Body.Text | This is a draft of the new home page. |
            | Autoroute.PromoteToHomePage | true |
        And I hit "Publish Now"
        And I go to "/"
    Then I should see "<h1>Super Duper</h1>"
    When I go to "super-duper"
    Then the status should be 404 "Not Found"

Scenario: A new page marked to be the home page but only saved as draft does not take over the home page
    Given I have installed Orchard
    When I go to "admin/contents/create/page"
        And I fill in
            | name | value |
            | Title.Title | Drafty |
            | Body.Text | This is a draft of the new home page. |
            | Autoroute.PromoteToHomePage | true |
        And I hit "Save"
        And I go to "/"
    Then I should see "<h1>Welcome to Orchard!</h1>"
