Feature: Content Page management
    In order to add content pages to my site
    As an author
    I want to create, publish and edit pages

Scenario: The "Page" content type is available to create from the admin menu
	Given I have installed Orchard
    When I go to ""
    Then I should see "<a href="/Admin/Contents/Create/Page">Page</a>"