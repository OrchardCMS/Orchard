Feature: Settings

In order to manage my site
As a privileged user
I want to be able to see and change site settings

Scenario: Adding a new site culture and selecting it as the default works

    Given I have installed Orchard
    When I go to "Admin/Settings/Index"
        Then I should not see "hu-HU"
    When I have "hu-HU" as the default culture
        And I go to "Admin/Settings/Index"
        Then I should see "<option selected="selected">hu-HU</option>"
