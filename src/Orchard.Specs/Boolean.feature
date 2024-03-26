Feature: Boolean Field
    In order to add boolean content to my types
    As an administrator
    I want to create, edit and publish boolean fields

Scenario: Creating and using Boolean fields
    
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
    
    # Adding a Boolean field
    When I go to "Admin/ContentTypes/Edit/Event"
        And I follow "Add Field"
        And I fill in
            | name          | value        |
            | DisplayName   | Active       |
            | Name          | Active       |
            | FieldTypeName | BooleanField |
        And I hit "Save"
        And I am redirected
    Then I should see "The \"Active\" field has been added."

    # Creating an Event content item
    When I go to "Admin/Contents/Create/Event"
    Then I should see "Active"
    When I fill in 
            | name				 | value |
            | Event.Active.Value | true  |
        And I hit "Save"
        And I am redirected
    Then I should see "Your Event has been created."
    When I go to "Admin/Contents/List"
    Then I should see "Active:" 
        And I should see "Yes"

    # The hint should be displayed
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                | value                        |
            | Fields[Active].BooleanFieldSettings.Hint | Check if the event is active |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "Check if the event is active"
    
    # The default value should be proposed on creation
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                        | value |
            | Fields[Active].BooleanFieldSettings.DefaultValue | True  |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
    Then I should see "checked=\"checked\""

    # The value should be required
    When I go to "Admin/ContentTypes/Edit/Event"
        And I fill in 
            | name                                        | value |
            | Fields[Active].BooleanFieldSettings.Optional     | false |
        And I hit "Save"
        And I go to "Admin/Contents/Create/Event"
        And I fill in 
            | name               | value |
            | Event.Active.Value |       |
        And I hit "Save"
    Then I should see "The field Active is mandatory."