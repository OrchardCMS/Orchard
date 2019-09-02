Feature: Setup
    In order to install orchard
    As a new user
    I want to access the Setup screen from the default screen and the Setup folder, then the Setup should be successful if required fields are filled in

Scenario: RootAndSetupFolderShowsSetupScreenAndFormValuesAreValidated
    Given I have a clean site with standard extensions

    When I go to "/"
    Then I should see "Welcome to Orchard"
        And I should see "Finish Setup"
        And the status should be 200 "OK"

    When I go to "/Setup"
    Then I should see "Welcome to Orchard"
        And I should see "Finish Setup"
        And the status should be 200 "OK"

    When I go to "/Setup"
        And I hit "Finish Setup"
    Then I should see "Site name is required."
        And I should see "Password is required."
        And I should see "Password confirmation is required."

    When I go to "/Setup"
        And I fill in 
            | name | value |
            | SiteName | My Site |
            | AdminPassword | 6655321 |
            | ConfirmPassword | 6655321 |
        And I hit "Finish Setup"
        And I go to "/"
    Then I should see "My Site"
        And I should see "Welcome, <strong><a href="/Users/Account/ChangePassword">admin</a></strong>!"