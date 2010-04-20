Feature: Setup
  In order to install orchard
  As a new user
  I want to setup a new site from the default screen

Scenario: Root request shows setup form
  Given I have a clean site
    And I have module "Orchard.Setup"
    And I have theme "SafeMode"
  When I go to "/Default.aspx"
  Then I should see "Welcome to Orchard"
    And I should see "Finish Setup"
    And the status should be 200 OK

Scenario: Setup folder also shows setup form
  Given I have a clean site
    And I have module "Orchard.Setup"
    And I have theme "SafeMode"
  When I go to "/Setup"
  Then I should see "Welcome to Orchard"
    And I should see "Finish Setup"
    And the status should be 200 OK

Scenario: Some of the initial form values are required
  Given I have a clean site
    And I have module "Orchard.Setup"
    And I have theme "SafeMode"
  When I go to "/Setup"
    And I hit "Finish Setup"
  Then I should see "Site name is required"
    And I should see "Password is required"

Scenario: Initializing the site
  Given I have a clean site
    And I have module "Orchard.Setup"
    And I have theme "SafeMode"
	And I am on "/Setup"
  When I fill in 
      | name | value |
      | SiteName | My Site |
      | AdminPassword | 6655321 |
    And I hit "Finish Setup"
  Then I should see "Blah"

