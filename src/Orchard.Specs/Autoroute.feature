Feature: Autoroutes
    In order to add content to my site
    As an author
    I want to create, publish and edit routes

Scenario: I can create and publish a new Page with international characters in its route
    Given I have installed Orchard
    When I go to "admin/contents/create/page"
        And I fill in
            | name | value |
            | Title.Title | Χελλο |
        And I hit "Publish"
        And I go to "Χελλο"
    Then I should see "<h1[^>]*>.*?Χελλο.*?</h1>"

Scenario: I can create and publish a new Home Page
    Given I have installed Orchard
    When I go to "admin/contents/create/page"
        And I fill in
            | name | value |
            | Title.Title | Foo |
            | AutoroutePart.PromoteToHomePage | True |
        And I hit "Publish"
        And I go to "/"
    Then I should see "<h1[^>]*>.*?Foo.*?</h1>"
    When I go to "/welcome-to-orchard"
    Then I should see "<h1[^>]*>.*?Welcome.*?</h1>"
