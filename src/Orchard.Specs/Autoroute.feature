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
        And I hit "Publish Now"
        And I go to "Χελλο"
    Then I should see "<h1[^>]*>.*?Χελλο.*?</h1>"
