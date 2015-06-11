Feature: Using SpecFlow
    In order to test Orchard
    As a developer or contributor
    I want to define scenarios that ensure functionality

Scenario: Spec flow generates and runs via nunit
    Given I have a scenario
    When I run steps
        And they have values like "5"
    Then they run
        And values like five are captured