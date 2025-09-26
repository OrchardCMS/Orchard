Feature: Module management
    In order add and enable features
    As a root Orchard system operator
    I want to install and enable modules and enable features

Scenario: Installed modules are listed
    Given I have installed Orchard
    When I go to "admin/modules"
    Then I should see "<h1 id="page-title">Modules</h1>"
    When I fill in
        | name | value |
        | Options.SearchText | Themes |
    And I hit "Search"
    Then I should see "<h1 id="page-title">Modules</h1>"
        And I should see "<h2[^>]*>\s*Themes"
        And the status should be 200 "OK"

Scenario: Features of installed modules are listed
    Given I have installed Orchard
    When I go to "admin/modules/features"
    Then I should see "<h3>\s*Common\s*</h3>"
        And I should see "<li class="feature enabled[^"]*" id="contents-feature"[^>]*>"
        And the status should be 200 "OK"