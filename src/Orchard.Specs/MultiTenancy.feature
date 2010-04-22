Feature: Multiple tenant management
	In order to host several isolated web applications
	As a root Orchard system operator
	I want to create and manage tenant configurations

Scenario: Default site is listed 
	Given I have installed Orchard
	  And I have installed "Orchard.MultiTenancy"
	When I go to "Admin/MultiTenancy"
	Then I should see "List of Site's Tenants"
	  And I should see "Default"
	  And the status should be 200 OK

