Feature: Module management
	In order add and enable features
	As a root Orchard system operator
	I want to install and enable modules and enable features

Scenario: Installed modules are listed
	Given I have installed Orchard
	When I go to "admin/modules"
	Then I should see "<h1>Installed Modules</h1>"
	    And I should see "<h3>Themes</h3>"
	    And the status should be 200 OK

Scenario: Edit module shows its features
    Given I have installed Orchard
    When I go to "admin/modules/Edit/Orchard.Themes"
    Then I should see "<h1>Edit Module: Themes</h1>"
        And the status should be 200 OK

Scenario: Features of installed modules are listed
    Given I have installed Orchard
    When I go to "admin/modules/features"
    Then I should see "<h1>Manage Features</h1>"
        And I should see "<h3>Common</h3>"
        And the status should be 200 OK