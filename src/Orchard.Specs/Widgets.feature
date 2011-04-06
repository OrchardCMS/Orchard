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

Scenario: I can add a new layer and that layer is active when I'm redirected to the widget management page
    Given I have installed Orchard
    When I go to "admin/widgets"
        And I follow "Add a new layer..."
    Then I should see "<h1[^>]*>Add Layer</h1>"
    When I fill in
            | name | value |
            | Name | For awesome stuff |
            | LayerRule | url "~/awesome*" |
        And I hit "Save"
        And I am redirected
    Then I should see "Your Layer has been created."
        And I should see "<option[^>]+selected="selected"[^>]+value="\d+">For awesome stuff</option>"

Scenario: I can delete a layer
    Given I have installed Orchard
    When I go to "admin/widgets"
    Then I should see "<option[^>]*>Default</option>"
    When I follow "Edit"
    Then I should see "<input[^>]*name="Name"[^>]*value="Default"[^>]*>"
    When I hit "Delete"
        And I am redirected
    Then I should see "Layer was successfully deleted"
        And I should not see "<option[^>]*>Default</option>"
