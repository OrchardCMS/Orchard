Feature: Text Field
    In order to add Text content to my types
    As an administrator
    I want to create, edit and publish Text fields

Scenario: Creating and using Text fields
    
    # Creating an Event content type 
    Given I have installed Orchard
        And I have installed "Orchard.Fields"
    When I go to "Admin/ContentTypes"
    Then I should see "<a[^>]*>.*?Create new type</a>"
    When I go to "Admin/ContentTypes/Create"
        And I fill in
            | name        | value |
            | DisplayName | Event |
            | Name        | Event |
        And I hit "Create"
        And I go to "Admin/ContentTypes/"
    Then I should see "Event"
    
    # Adding a Text field
    When I go to "Admin/ContentTypes/Edit/Event"
        And I follow "Add Field"
        And I fill in
            | name          | value     |
            | DisplayName   | Subject   |
            | Name          | Subject   |
            | FieldTypeName | TextField |
        And I hit "Save"
        And I am redirected
    Then I should see "The \"Subject\" field has been added."
    
    # The display option should be effective
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                                       | value |
            | Fields[Subject].TextFieldSettingsEventsViewModel.Settings.Flavor | Large |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "class=\"text large\""
    
    # The value should be required
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                                         | value |
            | Fields[Subject].TextFieldSettingsEventsViewModel.Settings.Required | true  |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
        And I fill in 
            | name               | value |
            | Event.Subject.Text |       |
        And I hit "Save"
    Then I should see "The field Subject is mandatory."

    # The hint should be displayed
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                                     | value                |
            | Fields[Subject].TextFieldSettingsEventsViewModel.Settings.Hint | Subject of the event |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "Subject of the event"
    
    # Creating an Event content item
    When I go to "Admin/Contents/Create/Event"
    Then I should see "Subject"
    When I fill in 
            | name               | value                |
            | Event.Subject.Text | Orchard Harvest 2015 |
        And I hit "Save"
        And I am redirected
    Then I should see "Your Event has been created."
        And I should see "Orchard Harvest 2015"

    # The default value should be proposed on creation
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                                             | value                |
            | Fields[Subject].TextFieldSettingsEventsViewModel.Settings.DefaultValue | Orchard Harvest 2016 |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "value=\"Orchard Harvest 2016\""

    # The required attribute should be used
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                                             | value |
            | Fields[Subject].TextFieldSettingsEventsViewModel.Settings.Required     | true  |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "required=\"required\""

    # The required attribute should not be used
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                                             | value |
            | Fields[Subject].TextFieldSettingsEventsViewModel.Settings.Required     | false |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should not see "required=\"required\""