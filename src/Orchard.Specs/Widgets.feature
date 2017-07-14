Feature: Widgets
    In order to add and manage widgets on my site
    As an author
    I want to create and edit widgets and layers

Scenario: I can edit a default layer
    Given I have installed Orchard
    When I go to "Admin/Widgets"
    Then I should see "<h1[^>]*>Widgets[^>]*>"
    When I follow "Edit"
    Then I should see "<input[^>]*name="LayerPart.Name"[^>]*value="Default"[^>]*>"
    When I fill in
            | name | value |
            | LayerPart.Description | This is the default layer. |
        And I hit "Save Draft"
        And I am redirected
    Then I should see "Your Layer has been saved"
    When I follow "Edit"
    Then I should see "<textarea[^>]*>\s*This is the default layer.\s*</textarea>"

Scenario: I can add a new layer and that layer is active when I'm redirected to the widget management page
    Given I have installed Orchard
    When I go to "Admin/Widgets"
        And I follow "Add a new layer..."
    Then I should see "<h1[^>]*>Add Layer</h1>"
    When I fill in
            | name | value |
            | LayerPart.Name | For awesome stuff |
            | LayerPart.LayerRule | url "~/awesome*" |
        And I hit "Save Draft"
        And I am redirected
    Then I should see "The Layer has been created as draft."
        And I should see "<option[^>]+selected="selected"[^>]+value="\d+">For awesome stuff</option>"

Scenario: I can delete a layer
    Given I have installed Orchard
    When I go to "Admin/Widgets"
    Then I should see "<option[^>]*>Default</option>"
    When I follow "Edit"
    Then I should see "<input[^>]*name="LayerPart.Name"[^>]*value="Default"[^>]*>"
    When I hit "Delete"
        And I am redirected
    Then I should see "Layer was successfully deleted"
        And I should not see "<option[^>]*>Default</option>"

Scenario: I can add a widget to a specific zone in a specific layer
    Given I have installed Orchard
    When I go to "Admin/Widgets"
        And I fill in
            | name | value |
            | layerId | Disabled |
        And I hit "Show"
    Then I should see "<option[^>]*selected[^>]*>Disabled"
    When I follow "Add" where href has "zone=Header"
    Then I should see "<h1[^>]*>Choose A Widget</h1>"
    When I follow "<h2>Html Widget</h2>"
    Then I should see "<h1[^>]*>Add Widget</h1>"
    When I fill in
            | name | value |
            | WidgetPart.Title | Flashy HTML Widget |
            | Body.Text | <p><blink>hi</blink></p> |
        And I hit "Save Draft"
        And I am redirected
    Then I should see "Your Html Widget has been added."
        And I should see "<option[^>]*selected[^>]*>Disabled"
        And I should see "<li[^>]*class="[^"]*widgets-this-layer[^"]*"[^>]*>\s*<form[^>]*>\s*<h3[^>]*>\s*<a[^>]*>Flashy HTML Widget</a>\s*</h3>"
