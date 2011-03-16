Feature: Widgets
    In order to add and manage widgets on my site
    As an author
    I want to create and edit widgets and layers

Scenario: I can edit a default layer
    Given I have installed Orchard
    When I go to "admin/widgets"
        And I follow "Edit"
    Then I should see "<input[^>]*name="Name"[^>]*value="Default"[^>]*>"
    When I fill in
            | name | value |
            | Description | This is the default layer. |
        And I hit "Save"
        And I am redirected
    Then I should see "Your Layer has been saved"
    When I follow "Edit"
    Then I should see "<textarea[^>]*>\s*This is the default layer.\s*</textarea>"
