Feature: The compilation of modules installed in a site
    In order to install on Orchard site
    As a privileged user
    I want to have the modules compiled/installed properly

Scenario: Dynamic compilation support: modules can be deployed as source files only
    Given I have chosen to deploy modules as source files only
        And I have installed Orchard
    When I go to "admin"
    Then I should see "<div id="orchard-version">Orchard v(?:\.\d+){2,4}</div>"