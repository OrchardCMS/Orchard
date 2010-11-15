Feature: Content Page management
    In order to add content pages to my site
    As an author
    I want to create, publish and edit pages

Scenario: In the admin (menu) there is a link to create a Page
	Given I have installed Orchard
    When I go to "admin"
    Then I should see "<a href="/Admin/Contents/Create/Page">Page</a>"

Scenario: I can create and publish a new Page
    Given I have installed Orchard
    When I go to "admin/contents/create/page"
        And I fill in
            | name | value |
            | Routable.Title | Super Duper |
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
            | Routable.Title | Super Duper |
            | Body.Text | This is super. |
        And I hit "Publish Now"
        And I go to "super-duper"
    Then I should see "<h1[^>]*>.*?Super Duper.*?</h1>"
        And I should see "This is super."
    When I go to "admin/contents/create/page"
        And I fill in
            | name | value |
            | Routable.Title | Super Duper |
            | Body.Text | This is super number two. |
        And I hit "Publish Now"
        And I go to "super-duper-2"
    Then I should see "<h1[^>]*>.*?Super Duper.*?</h1>"
        And I should see "This is super number two."