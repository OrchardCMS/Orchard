Feature: The Admin side of the app
    In order to manage my site
    As a privileged user
    I want to not have my cheese moved in the admin

Scenario: The current version of Orchard is displayed in the admin
    Given I have installed Orchard
    When I go to "admin"
    Then I should see "<div id="orchard-version">Orchard v(?:\.\d+){2,4}</div>"