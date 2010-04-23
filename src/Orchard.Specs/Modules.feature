Feature: Module management
	In order add and enable features
	As a root Orchard system operator
	I want to install and enable modules and enable features

Scenario: Default modules are listed
	Given I have installed Orchard
	When I go to "admin/modules"
	Then I should see "Installed Modules"
	    And I should see "<h3>Themes</h3>"
	    And the status should be 200 OK
