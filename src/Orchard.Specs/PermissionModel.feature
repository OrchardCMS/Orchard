Feature: Addition
    In order to prevent security model regressions
    As a user with specific permissions
    I should to be granted or denied access to various actions

@security
Scenario: Login can be automated
    Given I have installed Orchard
        And I have a user "bob" with permissions "AccessFrontEnd"
    When I go to "users/account/logoff"
        And I go to "users/account/logon"
        And I fill in
            | name | value |
            | userNameOrEmail | bob |
            | password | qwerty123! |
        And I hit "Sign In"
        And I am redirected
    Then I should see "Welcome"
        And I should see "bob"

@security
Scenario: Anonymous user can see the home page but not the dashboard
    Given I have installed Orchard
    And I have a user "bob" with permissions "AccessFrontEnd"
    When I sign in as "bob"
     And I go to "/"
    Then I should see "this is the homepage of your new site"
     And I should be denied access when I go to "admin"