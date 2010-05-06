Feature: Profiling
	In order to profile the site
	As a developer
	I want to generate a fixed number of repeatable requests

Scenario: Dashboard
	Given I am logged in
	When I go to "/admin" 40 times
